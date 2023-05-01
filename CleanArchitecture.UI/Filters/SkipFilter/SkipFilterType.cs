using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.SkipFilter
{
    public class SkipFilterType : Attribute, IFilterMetadata
    {
    }
}
