namespace HKFps
{
    public class WaterBottleItem : ItemBase
    {
        public int HealthAddition = 20;

        public override void StartUse(HKPlayerItemSystem controller)
        {

        }

        public override void HoldUse(HKPlayerItemSystem controller)
        {

        }

        public override void ReleaseUse(HKPlayerItemSystem controller)
        {
            gameObject.SetActive(false);
            controller.AddHealth(HealthAddition);
            controller.OnUseComplete();
        }

        public override void CancelUse(HKPlayerItemSystem controller)
        {

        }
    }
}