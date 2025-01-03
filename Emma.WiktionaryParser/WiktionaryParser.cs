using Emma.Lib;
using System.Text.RegularExpressions;
using System.Xml;

namespace Emma.WiktionaryParser;

public class WiktionaryParser
{
    static readonly Regex allowedTitle = new("^[a-z'-]+$");


    public static DefinitionSet ParseXmlData(string xmlFilename)
    {
        Console.WriteLine("Reading Wiktionary XML file: " + xmlFilename);

        using var xml = XmlReader.Create(xmlFilename);
        var result = new DefinitionSet();

        const string skipTo = "";
        bool skip = true;
        int defs = 0;

        while (xml.ReadToFollowing("page"))
        {
            xml.ReadToFollowing("title");
            string word = xml.ReadElementContentAsString();
            if (!allowedTitle.IsMatch(word)) continue;

            if (skipTo == "" || skipTo == word)
            {
                skip = false;
            }

            if (skip) continue;

            xml.ReadToFollowing("ns");
            if (xml.ReadElementContentAsString() != "0") continue;

            xml.ReadToFollowing("text");
            string wikitext = xml.ReadElementContentAsString();

            foreach (var definition in ReadPage(word, wikitext))
            {
                defs++;

                if (defs % 100000 == 0)
                {
                    Console.WriteLine("..." + defs);
                }

                result.Add(definition);
            }
        }

        return result;
    }


    static readonly Regex comment = new("<!--.+?-->", RegexOptions.Singleline);
    static readonly Regex reference = new("<ref>.+?</ref>", RegexOptions.Singleline);
    static readonly Regex reference2 = new("<ref name[^<>]+?/>", RegexOptions.Singleline);
    static readonly Regex reference3 = new("<ref name[^<>]+?>.*?</ref>", RegexOptions.Singleline);
    static readonly Regex languageHeader = new(@"^= *= *[^=]+ *=");
    static readonly Regex englishHeader = new("^= *= *English *=");
    static readonly Regex lowerHeader = new("^(?:= *){3,}([^=]+) *=");
    static readonly Regex definitionLine = new("^( *#)+ ");


    static IEnumerable<Definition> ReadPage(string word, string wikitext)
    {
        var previousDefs = new HashSet<string>();
        bool english = false;
        string partOfSpeech = "";
        bool offensive = false;

        string cleanWikitext = wikitext;
        cleanWikitext = comment.Replace(cleanWikitext, "");
        cleanWikitext = reference.Replace(cleanWikitext, "");
        cleanWikitext = reference2.Replace(cleanWikitext, "");
        cleanWikitext = reference3.Replace(cleanWikitext, "");

        using var sr = new StringReader(cleanWikitext);

        var defs = new List<Definition>();

        while (sr.ReadLine() is string line)
        {
            if (englishHeader.IsMatch(line))
            {
                english = true;
            }
            else if (languageHeader.IsMatch(line))
            {
                english = false;
            }

            if (english)
            {
                if (lowerHeader.Match(line) is Match m && m.Success)
                {
                    string heading = m.Groups[1].Value.ToLower();

                    if (heading.IndexOf("<") is int pos && pos > 0)
                    {
                        heading = heading[..pos];
                    }

                    if (heading.StartsWith("etymology") || heading == "pronunciation" ||
                        heading == "alternative forms" || heading == "references" ||
                        heading == "proper noun" || heading == "further reading" ||
                        heading == "derived terms" || heading == "hyponyms" ||
                        heading == "usage notes" || heading == "synonyms" ||
                        heading == "related terms" || heading == "see also")
                    {
                        partOfSpeech = "";
                    }
                    else
                    {
                        partOfSpeech = heading;
                    }
                }

                if (english && partOfSpeech != "" && definitionLine.IsMatch(line))
                {
                    if (line.Contains("offensive", StringComparison.OrdinalIgnoreCase) ||
                        line.Contains("vulgar", StringComparison.OrdinalIgnoreCase) ||
                        line.Contains("euphemistic", StringComparison.OrdinalIgnoreCase) ||
                        line.Contains("pejorative", StringComparison.OrdinalIgnoreCase))
                    {
                        offensive = true;
                    }

                    var def = ParseDefinition(word, partOfSpeech, line);

                    if (def != null)
                    {
                        if (previousDefs.Add(def.Content))
                        {
                            defs.Add(def);
                        }
                    }
                }
            }
        }

        foreach (var def in defs)
        {
            if (offensive)
            {
                def.Offensive = true;
            }

            yield return def;

            if (def.Pos == PartOfSpeech.Noun)
            {
                string possessive = def.Word.EndsWith('s') ? def.Word + "'" : def.Word + "'s";

                var possessiveDef = new Definition(possessive, PartOfSpeech.Noun, possessive, "Possessive form of " + def.Word.ToUpper())
                {
                    Offensive = offensive
                };

                yield return possessiveDef;
            }
        }
    }


    static readonly Regex template = new(@"{{([^{|}]+?)" + string.Join("", Enumerable.Repeat("(?:\\|([^{|}]*?))?", 10)) + "}}");
    static readonly Regex pipedLink = new(@"\[\[[^|\]]+?\|([^]]+?)\]\]");
    static readonly Regex link = new(@"\[\[([^|\]]+?)\]\]");
    static readonly Regex senseText = new(@"\[?\(?\""?\(? ?(noun|verb)? ?sense ?[0-9\.]+ ?\)?\""?\)?\]?");
    static readonly Regex bold = new("'''([^']+?)'''");
    static readonly Regex italics = new("''([^']+?)''");
    static readonly Regex subscript = new("<sub>[^<]+?</sub>");
    static readonly Regex superscript = new("<sup>[^<]+?</sup>");
    static readonly Regex multipleSpaces = new("  +");


    static Definition? ParseDefinition(string word, string partOfSpeech, string line)
    {
        int start = 2, end = line.Length;

        while (start < line.Length && (line[start] == '#' || char.IsWhiteSpace(line[start])))
        {
            start++;
        }

        while (char.IsWhiteSpace(line[end - 1]))
        {
            end--;
        }

        if (end <= start) return null;

        string definition = line[start..end];
        string? see = null;

        definition = pipedLink.Replace(definition, "$1");
        definition = link.Replace(definition, "$1");

        while (template.Match(definition) is Match match && match.Success)
        {
            string? result = ParseTemplate(word, match, ref see);

            if (result == null)
            {
                return null;
            }

            definition = definition.Remove(match.Index, match.Length).Insert(match.Index, result);
        }

        definition = bold.Replace(definition, "\"$1\"");
        definition = italics.Replace(definition, "\"$1\"");
        definition = subscript.Replace(definition, "$1");
        definition = superscript.Replace(definition, "$1");

        definition = senseText.Replace(definition, "");

        /*definition = templateSee.Replace(definition, m =>
        {
            see = m.Groups[1].Value.ToUpper();
            return $"See {see}";
        });*/

        definition = multipleSpaces.Replace(definition, " ");

        definition = definition.Replace("“", "\"");
        definition = definition.Replace("”", "\"");
        definition = definition.Replace("\"\"", "\"");
        definition = definition.Replace(".)", ").");
        definition = definition.Replace(" ,", ",");

        definition = definition.Trim(' ', ':', ' ', '.');

        if (definition.StartsWith('(') && definition.EndsWith(')'))
        {
            return null;
        }

        definition += ".";
        definition = char.ToUpper(definition[0]) + definition[1..];

        if (see != null)
        {
            string lower = see.ToLower();
            string upper = see.ToUpper();

            if (allowedTitle.IsMatch(lower))
            {
                see = lower;
            }
            else
            {
                see = null;

                if (upper != "")
                {
                    definition = definition.Replace(upper, $"\"{lower}\"");
                }
            }
        }

        partOfSpeech = partOfSpeech.ToLower() switch
        {
            "prepositional phrase" => "preposition",

            "numeral" or "number" or "letter" or
            "symbol" or "punctuation mark" => "noun",

            _ => partOfSpeech
        };

        if (!Enum.TryParse<PartOfSpeech>(partOfSpeech, true, out var pos))
        {
            pos = PartOfSpeech.Undefined;
        }

        return new Definition(word, pos, see, definition);
    }


    static string? ParseTemplate(string word, Match match, ref string? see)
    {
        var args = new List<string>();

        for (int i = 1; i < match.Groups.Count; i++)
        {
            if (!match.Groups[i].Success)
            {
                break;
            }

            string arg = match.Groups[i].Value;

            if (arg.IndexOf('=') is int pos && pos > 0)
            {
                string argname = arg[0..pos];
                string argValue = arg[(pos + 1)..];

                if (int.TryParse(argname, out int n))
                {
                    while (args.Count <= n)
                    {
                        args.Add("");
                    }

                    args[n] = argValue;
                }
            }
            else
            {
                args.Add(arg);
            }
        }

        string templateName = args[0].ToLower().Replace("  ", " ").Replace("-lite", "").TrimEnd(' ', '2');

        try
        {

            if (templateName.Contains(':') ||
                templateName.StartsWith("rf") ||
                templateName.StartsWith('+'))
            {
                return "";
            }
            else if (templateName.EndsWith("-l") || templateName.EndsWith("-inline"))
            {
                return args[1];
            }
            else if (templateName.EndsWith("-def"))
            {
                return args.Count switch
                {
                    2 => args[1],
                    3 or 4 => $"The {args[2]} {word.ToUpper()}",
                    _ => $"The {args[2]} {args[4]}"
                };
            }
            else if (templateName == "infl of" || templateName == "inflection of")
            {
                string def = "";

                foreach (string arg in args.Skip(4))
                {
                    var tags = arg.Split("//").Select(x => x.Trim()).ToArray();

                    for (int i = 0; i < tags.Length; i++)
                    {
                        string? form = tags[i] switch
                        {
                            "sim" or "simple" => "simple",
                            "pres" or "present" => "present",
                            "part" or "participle" => "participle",

                            "p" or "pl" or "plural" => "plural",
                            "s" => "singular",
                            "1" => "first-person",
                            "2" => "second-person",
                            "3" => "third-person",
                            "comd" => "comparative",
                            "supd" => "superlative",
                            "imp" => "imperative",
                            "ind" or "indc" => "indicative",
                            "cond" => "conditional",
                            "sub" => "subjunctive",
                            "an" => "animate",

                            "obs" => "(obsolete)",

                            "past" or "tense" or "form" or "alternative" => tags[i],

                            ";" => "and",

                            _ => tags[i]
                        };

                        if (form != null)
                        {
                            def += " ";
                            def += form;

                            if (i < tags.Length - 1)
                            {
                                def += ",";
                            }
                        }
                    }
                }

                def = def.Trim();

                if (def.EndsWith("past") || def.EndsWith("present"))
                {
                    def += " tense";
                }
                else if (!def.EndsWith("form") && !def.EndsWith("participle") && !def.EndsWith("tense"))
                {
                    def += " form";
                }

                def += " of ";
                def += see = args[2].ToUpper();

                return def.ToString();
            }
            else
            {
                return templateName switch
                {
                    "g" or "ng" or "n-g" or "non-gloss" or "ngd" or "non-gloss definition" or "non gloss definition" or
                    "taxlink" or "vern" or "math" or
                    "unsupported" or "nobold" or "upright" or
                    "monospace" or "nowrap" or "sub" or "sup" or "small" or
                    "ipalink" or "pedlink"
                        => args[1],

                    "ipafont" => $"\"{args[1]}\"",

                    "smallcaps" => args[1].ToUpper(),

                    "quote" or "ll" or "lang" or "t" => args[2],

                    "cog" or "nc" => $"\"{args[2]}\"",

                    "der" => $"{args[2].ToUpper()}",

                    "l" or "link" or
                    "glossary" => args[^1],

                    "m" or "m+" or "mention" or "m-self" or
                    "pedia" => $"\"{args[^1]}\"",

                    "i" or "q" or "qf" or "qual" or "qualifier" or
                    "gl" or "gloss" or "s" or "sense" => $"({args[1]})",

                    "alt" or "alter" => string.Join(", ", args.Skip(2)),

                    "lb" or "lbl" or "label" or "context" or
                    "tlb" or "term-label" or "ic" or "ipachar" or "a" =>
                        "(" + string.Join(", ", args.Skip(2).Where(a => a != "_")) + ")",

                    "w" => args[args.Count > 2 ? 2 : 1],

                    "1" => char.ToUpper(args[1][0]) + args[1][1..],

                    "frac" => args.Count == 3 ? $"{args[1]}/{args[2]}" : $"1/{args[1]}",

                    "nuclide" => $"({args[1]},{args[2]}){args[3]}",

                    "indtr" => $"(transitive with \"{args[2]}\")",

                    "sumti" => $"x{args[1]}",
                    "lit" => $"lit. \"{args[1]}\"",

                    "agent noun of" => $"Agent noun of {see = args[2].ToUpper()}",
                    "altform" or "alt form" or "alt form of" or "alternative form" or "alternative form of" => $"Alternative form of {see = args[2].ToUpper()}",
                    "alternative plural of" => $"Alternative plural form of {see = args[2].ToUpper()}",

                    "altsp" or "alt sp" or "altspell" or "alt spell" or "alt spelling" or
                    "alt spelling of" or "alternative spelling of" or "doublet"
                        => $"Alternative spelling of {see = args[2].ToUpper()}",

                    "archaic form of" => $"Archaic form of {see = args[2].ToUpper()}",
                    "arc sp" or "archaic spelling of" => $"Archaic spelling of {see = args[2].ToUpper()}",
                    "attributive form of" => $"Attributive form of {see = args[2].ToUpper()}",
                    "cens sp" or "censored spelling of" => $"Censored spelling of {see = args[2].ToUpper()}",
                    "dated form" or "dated form of" => $"Dated form of {see = args[2].ToUpper()}",
                    "dated spelling of" => $"Dated spelling of {see = args[2].ToUpper()}",
                    "deliberate misspelling of" or "intentional misspelling of" => $"Deliberate misspelling of {see = args[2].ToUpper()}",
                    "diminutive of" => $"Diminutive of {see = args[2].ToUpper()}",
                    "eggcorn of" => $"Eggcorn of {see = args[2].ToUpper()}",
                    "euph form" or "euphemism of" or "euphemistic form of" => $"Euphemistic form of {see = args[2].ToUpper()}",
                    "eye dialect of" => $"Eye dialect spelling of {see = args[2].ToUpper()}",
                    "fa sp" or "filter-avoidance spelling of" => $"Filter-avoidance spelling of {see = args[2].ToUpper()}",
                    "former name of" => $"Former name of {see = args[2].ToUpper()}",
                    "informal form of" => $"Informal form of {see = args[2].ToUpper()}",
                    "informal spelling of" => $"Informal spelling of {see = args[2].ToUpper()}",
                    "missp" or "misspelling of" => $"Misspelling of {see = args[2].ToUpper()}",
                    "nonstandard form of" => $"Nonstandard form of {see = args[2].ToUpper()}",
                    "nonstandard spelling of" => $"Nonstandard spelling of {see = args[2].ToUpper()}",
                    "obs form" or "obsolete form of" => $"Obsolete form of {see = args[2].ToUpper()}",
                    "obs sp" or "obsolete spelling of" => $"Obsolete spelling of {see = args[2].ToUpper()}",
                    "obsolete typography of" => $"Obsolete typography of {see = args[2].ToUpper()}",
                    "pronunciation spelling of" => $"Pronunciation spelling of {see = args[2].ToUpper()}",
                    "rare form" or "rare form of" => $"Rare form of {see = args[2].ToUpper()}",
                    "rare sp" or "rare spelling of" => $"Rare spelling of {see = args[2].ToUpper()}",
                    "standard form of" => $"Standard form of {see = args[2].ToUpper()}",
                    "stand sp" or "standard spelling of" => $"Standard spelling of {see = args[2].ToUpper()}",
                    "syn of" or "synonym of" => $"Synonym of {see = args[2].ToUpper()}",
                    "uncommon form" or "uncommon form of" => $"Uncommon form of {see = args[2].ToUpper()}",
                    "uncommon sp" or "uncommon spelling of" or "less common spelling of" => $"Uncommon spelling of {see = args[2].ToUpper()}",
                    "verbal noun of" => $"Verbal noun of {see = args[2].ToUpper()}",

                    "comparative of" => $"Comparative form of {see = args[2].ToUpper()}",
                    "feminine singular of" => $"Alternative singular form of {see = args[2].ToUpper()}",
                    "feminine plural of" => $"Alternative plural form of {see = args[2].ToUpper()}",
                    "femeq" or "female equivalent of" => $"Alternative form of {see = args[2].ToUpper()}",
                    "genitive of" => $"Genitive form of {see = args[2].ToUpper()}",
                    "gerund of" => $"Gerund of {see = args[2].ToUpper()}",
                    "masculine of" or "masculine noun of" => $"Alternative form of {see = args[2].ToUpper()}",
                    "masculine plural of" => $"Alternative plural form of {see = args[2].ToUpper()}",
                    "past participle of" => $"Past participle of {see = args[2].ToUpper()}",
                    "past tense of" => $"Past tense of {see = args[2].ToUpper()}",
                    "plural" or "plural of" => $"Plural form of {see = args[2].ToUpper()}",
                    "present participle of" => $"Present participle of {see = args[2].ToUpper()}",
                    "present tense of" => $"Present tense of {see = args[2].ToUpper()}",
                    "singular of" => $"Singular form of {see = args[2].ToUpper()}",
                    "singulative of" => $"Singulative form of {see = args[2].ToUpper()}",
                    "superlative of" => $"Superlative form of {see = args[2].ToUpper()}",

                    "en-archaic past of" => $"(archaic) simple past tense and past participle of {see = args[1].ToUpper()}",
                    "en-archaic second-person singular of" => $"(archaic) Second-person singular simple present form of {see = args[1].ToUpper()}",
                    "en-archaic second-person singular past of" => $"(archaic) Second-person singular simple past form of {see = args[1].ToUpper()}",
                    "en-archaic third-person singular of" => $"(archaic) third-person singular simple present indicative form of {see = args[1].ToUpper()}",
                    "en-comparative of" => $"Comparative form of {see = args[1].ToUpper()}",
                    "en-ing" or "en-ing form of" => $"Present participle and gerund of {see = args[1].ToUpper()}",
                    "en-ipl" or "en-irregular plural of" => $"Plural form of {see = args[1].ToUpper()}",
                    "en-obsolete past participle of" => $"(obsolete) Past participle of {see = args[1].ToUpper()}",
                    "en-past of" => $"Simple past tense and past participle of {see = args[1].ToUpper()}",
                    "en-simple past of" => $"Simple past tense of {see = args[1].ToUpper()}",
                    "en-superlative of" => $"Superlative form of {see = args[1].ToUpper()}",
                    "en-tpso" or "en-third-person singular of" or "en-third person singular of"
                        => $"Third-person singular simple present indicative form of {see = args[1].ToUpper()}",

                    "form of" => $"{args[2]} of \"{(args.Count > 4 ? args[4] : args[3])}\"",

                    "&lit" => $"Used other than figuratively or idiomatically: see {string.Join(", ", args.Skip(2))}",

                    "abbr of" or "abbrev of" or "abbreviation of" => $"Abbreviation of \"{args[2]}\"",
                    "acronym of" => $"Acronym of \"{args[2]}\"",
                    "altcaps" or "alt caps" or "alt case" or "alternative case form of" or "alternative letter-case form of" => $"Alternative case form of \"{args[2]}\"",
                    "aphetic form of" => $"Aphetic form of \"{args[2]}\"",
                    "apocopic form of" => $"Apocopic form of \"{args[2]}\"",
                    "clipping" or "clip of" or "clipping of" => $"Clipping of \"{args[2]}\"",
                    "construed with" => $"Construed with \"{args[2]}\"",
                    "contraction of" => $"Contraction of \"{args[2]}\"",
                    "ellipsis" or "ellipsis of" => $"Ellipsis of \"{args[2]}\"",
                    "elongated form of" => $"Elongated form of \"{args[2]}\"",
                    "init of" or "initialism of" => $"Initialism of \"{args[2]}\"",
                    "misconstruction of" => $"Misconstruction of \"{args[2]}\"",
                    "only in" or "only used in" => $"Only used in \"{args[2]}\"",
                    "phrasal verb" or "used in phrasal verbs" => $"Used in a phrasal verb: \"{args[1]}\"",
                    "short for" => $"Short for \"{args[2]}\"",
                    "syncopic form of" => $"Syncopic form of \"{args[2]}\"",
                    "reduplication" => $"Reduplication of \"{args[2]}\"",

                    "senseid" or "senseno" or "etymid" or "anchor" or
                    "defdt" or "datedef" or "defdate" or "def-date" or
                    "color panel" or "colour panel" or "colorbox" or
                    "c" or "top" or "topic" or "topics" or "cln" or
                    "attention" or "attn" or
                    "wp" or "wikipedia" or "slim-wikipedia" or
                    "century" or "circa" or "c." or
                    "quote-web" or "quote-book" or
                    "tea room sense" or "isbn" or
                    "syn" or "synonyms" or "syndiff" or "ux" or
                    "ant" or "antonyms" or
                    "lr"
                        => "",

                    "si-unit" or "si-unit-" or "si-unit-abb" or "si-unit-np" => null,

                    "hot sense" => null,

                    "nbsp" => " ",
                    "!" => "|",
                    "bc" or "b.c." or "bce" or "b.c.e." => "BCE",
                    "a.d." or "ce" or "c.e." => "CE",
                    "sic" => "[sic]",
                    "," or "=" or "..." => templateName,

                    _ => null
                };
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}

