namespace ServiceItemsPlanningPlugin.Messages
{
    using Microting.ItemsPlanningBase.Infrastructure.Data.Entities;

    public class ScheduledItemExecuted
    {
        public int itemListId { get; protected set; }

        public ScheduledItemExecuted(int itemListId)
        {
            this.itemListId = itemListId;
        }
    }
}
