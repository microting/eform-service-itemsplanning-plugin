namespace ServiceItemsPlanningPlugin.Messages
{
    using Microting.ItemsPlanningBase.Infrastructure.Data.Entities;

    public class ScheduledItemExecuted
    {
        public ItemList itemList { get; protected set; }

        public ScheduledItemExecuted(ItemList itemList)
        {
            this.itemList = itemList;
        }
    }
}
