# EmmaWords
Software for playing, learning and analyzing word games, used by Emma on her stream.

The repository contains:
 - Emma.Anagramming - anagramming game, similar to Infiniwords.
 - Emma.IsBot - twitch bot, used by the emma_is_bot account.
 - Emma.Lib - shared library for the other projects.
 - Emma.MacondoParser - routines for parsing Macondo self-play logs.
 - Emma.Scrabble - basic Scrabble game implementation that supports some variant rules.
 - Emma.Stream - stream start screen, chat overlay, stream-specific bot commands etc.
 - Emma.WiktionaryParser - routines for turning a Wiktionary database dump into a set of machine-readable definitions.
 - Emma.WordLearner - word learning program

If you just want to run it, download from here: https://github.com/emma-britton/EmmaWords/releases/download/v0.1/EmmaWords.v0.1.zip

This was all designed for my personal use so many not be suitable for non-technical users. However it should be possible for anyone to run the Scrabble game or the word learner.

I will consider any change requests submitted via GitHub's issue tracking or pull request features.

# Word learning program
Select a lexicon and click 'Start'. The program will show you an alphagram. Enter the word it represents and press Enter. If there are multiple words, submit them one at a time, or if you prefer you can submit them all at once separated by spaces. Once you have entered all words correctly, it will move on to the next alphagram. If you enter any incorrect guesses these will be shown on the left in red. To give up on a particular alphagram, press Escape. Any words you missed are shown on the left in yellow. To continue, you'll need to type them in first - the idea is this helps you remember them in future.

The words are shown in order of how useful they are to know, based on data compiled from millions of games of Macondo playing against itself. This aligns with the conventional wisdom of how to learn words, with the most important being the 2-letter words, 3- to 5-letter words with J/Q/Z/X, and high probability 7 and 8 letter words.

The program will show you a mixture of new words, and words you have seen before. If you get the word right you will see it less often, if you get it wrong you will see it more often. The idea is that over time, it focuses on the words you have most trouble with. In the top right corner are some statistics - words are considered 'learned' once you've got them right twice in a row, 'in progress' if you got them right once, and 'missed' otherwise.

On the start screen there are two other options. A slider to control how often you review words you have seen before, and the number of net correct answers needed before it decides you know the word well enough and removes it from the list entirely. By default this is set to 3. If you prefer not to review words, you can set this to 1 and correctly answered words will be removed immediately (if you get them wrong, you'll then need to get them right twice to be removed, 2 wrong guesses means 3 correct answers needed, and so forth).

# Anagramming game
This is designed to be played on a Twitch stream with your viewers - the program only shows the anagrams and doesn't have a way to enter the answers directly.
You need to have a Twitch bot account set up and then enter the credentials for it. Any words typed in Twitch chat will then be picked up as potential answers. Each viewer gains points for correct answers (equal to the number of letters in the answered word) and loses 1 point per incorrect guess.

# Scrabble game
The game UI is intended for demonstration in a video/stream - it is not intended as a normal multi-player Scrabble game, but as a tool for exploring variants.
Only one player's rack is visible at a time and there is no way to hide the rack. 

Many aspects of the game can be customized:
 - Number of tiles on the rack
 - Board size
 - Arrangement of bonus spaces on the board
 - Number and point value of each tile
 - Whether word validation is applied

When editing the tiles, there are four columns. The column headed 'Tile' specifies what letter(s) the tile stands for, with blank being represented by a question mark, and the 'Display' column specifies what appears on the tile. This allows you to make tiles that represent multiple letters.

Ticking 'If only' variant allows you to designate one tile per turn as a blank, if you do not hold a normal blank tile on your rack. The game will do this automatically when you type in a letter that you do not have available to play.

# Using a custom lexicon
The repository comes with three lists (latest available versions of CEL, NWL and CSW). You can add your own by making a new text file in the 'lexicon' folder and add the words you want, one per line. This is then available for selection in the anagramming game, word learner, Scrabble game and in the definition bot (using the lexicon command).


