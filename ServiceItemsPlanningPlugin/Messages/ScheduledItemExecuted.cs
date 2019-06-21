namespace ServiceItemsPlanningPlugin.Messages
{
    public class ScheduledItemExecuted
    {
        public int itemListId { get; }

        public ScheduledItemExecuted(int itemListId)
        {
            this.itemListId = itemListId;
        }
    }
}
