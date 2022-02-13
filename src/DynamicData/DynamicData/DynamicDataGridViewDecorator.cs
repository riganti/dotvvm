using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls.DynamicData.Metadata;
using DotVVM.Framework.Controls.DynamicData.PropertyHandlers.GridColumns;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls.DynamicData
{
    public class DynamicDataGridViewDecorator : Decorator
    {
        /// <summary>
        /// Gets or sets the view name (e.g. Insert, Edit, ReadOnly). Some fields may have different metadata for each view.
        /// </summary>
        public string ViewName
        {
            get { return (string)GetValue(ViewNameProperty); }
            set { SetValue(ViewNameProperty, value); }
        }

        public static readonly DotvvmProperty ViewNameProperty
            = DotvvmProperty.Register<string, DynamicDataGridViewDecorator>(c => c.ViewName, null);

        /// <summary>
        /// Gets or sets whether the columns are generated on the left side or on the right side of the grid.
        /// </summary>
        public ColumnPlacement ColumnPlacement
        {
            get { return (ColumnPlacement)GetValue(ColumnPlacementProperty); }
            set { SetValue(ColumnPlacementProperty, value); }
        }

        public static readonly DotvvmProperty ColumnPlacementProperty
            = DotvvmProperty.Register<ColumnPlacement, DynamicDataGridViewDecorator>(c => c.ColumnPlacement, ColumnPlacement.Left);

        protected override void OnInit(IDotvvmRequestContext context)
        {
            base.OnInit(context);

            GenerateGridViewColumns(context);
        }

        /// <summary>
        /// Generates the GridView columns.
        /// </summary>
        private void GenerateGridViewColumns(IDotvvmRequestContext context)
        {
            var grid = FindGridView();

            var itemDataContextStack = grid.GetItemDataContextStack(ItemsControl.DataSourceProperty);
            var dynamicDataContext = new DynamicDataContext(itemDataContextStack, context.Services)
            {
                ViewName = ViewName
            };

            // generate columns
            var entityPropertyListProvider = context.Configuration.ServiceProvider.GetService<IEntityPropertyListProvider>();
            var viewContext = dynamicDataContext.CreateViewContext();
            var entityProperties = entityPropertyListProvider.GetProperties(itemDataContextStack.DataContextType, viewContext);

            // create columns
            var newColumns = new List<GridViewColumn>();
            foreach (var property in entityProperties)
            {
                // get the column provider
                var columnProvider = FindGridColumnProvider(dynamicDataContext, property);
                if (columnProvider == null) continue;

                // create the column
                var column = columnProvider.CreateColumn(grid, property, dynamicDataContext);
                SetColumnCommonProperties(grid, property, column, dynamicDataContext);

                // add the column to the GridView
                newColumns.Add(column);
            }
            AddColumns(grid, newColumns);
        }

        protected virtual void AddColumns(GridView grid, List<GridViewColumn> newColumns)
        {
            if (ColumnPlacement == ColumnPlacement.Left)
            {
                grid.Columns.InsertRange(0, newColumns);
            }
            else
            {
                grid.Columns.AddRange(newColumns);
            }
        }

        protected virtual GridView FindGridView()
        {
            try
            {
                return Children.OfType<GridView>().Single();
            }
            catch (InvalidOperationException)
            {
                throw new DotvvmControlException(this, $"The {nameof(DynamicDataGridViewDecorator)} control must have exactly one {nameof(GridView)} control inside!");
            }
        }

        /// <summary>
        /// Sets the common properties of the grid view column.
        /// </summary>
        protected virtual void SetColumnCommonProperties(GridView grid, PropertyDisplayMetadata property, GridViewColumn column, DynamicDataContext dynamicDataContext)
        {
            column.HeaderText = property.DisplayName;
        }

        /// <summary>
        /// Finds the grid column provider.
        /// </summary>
        protected virtual IGridColumnProvider FindGridColumnProvider(DynamicDataContext dynamicDataContext, PropertyDisplayMetadata property)
        {
            return dynamicDataContext.DynamicDataConfiguration.GridColumnProviders.FirstOrDefault(p => p.CanHandleProperty(property.PropertyInfo, dynamicDataContext));
        }
    }
}
