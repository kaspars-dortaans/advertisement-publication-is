﻿namespace BusinessLogic.Dto.DataTableQuery
{
    public class DataTableQueryConfig<Entity> where Entity : class
    {
        public IDictionary<string, Func<Entity, dynamic>> ValueSelectors { get; set; } = new Dictionary<string, Func<Entity, dynamic>>();
    }
}
