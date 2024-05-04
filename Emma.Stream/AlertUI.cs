using Emma.Lib;

namespace Emma.Stream;

public class AlertUI : Gdi
{
    private Queue<Alert> Queue = new();
    private Alert? CurrentAlert;
    private DateTime NextAlertTime;


    public AlertUI(Form owner) : base(owner)
    {
        BackColor = Color.FromArgb(0, 255, 0);
    }


    public void AddAlert(Alert alert)
    {
        Queue.Enqueue(alert);
    }


    public override void Tick()
    {
        if (CurrentAlert == null && NextAlertTime < DateTime.Now)
        {
            if (Queue.TryDequeue(out var alert))
            {
                CurrentAlert = alert;
            }
        }

        if (CurrentAlert != null)
        {
            CurrentAlert.Tick++;

            if (CurrentAlert.Done)
            {
                CurrentAlert = null;
                NextAlertTime = DateTime.Now.AddSeconds(5);
            }
        }
    }


    public override void Render()
    {
        if (CurrentAlert != null)
        {
            CurrentAlert.Render(this);
        }
    }
}


public class Alert
{
    public int Tick = 0;
    public bool Done;

    public virtual void Render(Gdi gdi)
    {
    }
}


public class FollowAlert : Alert
{
    public override void Render(Gdi gdi)
    {
        gdi.DrawFitText(Tick.ToString(), "Arial", Color.Black, gdi.Area, gdi.CenterCenter);

        if (Tick >= 100)
        {
            Done = true;
        }
    }
}