using System.Windows.Annotations;

namespace Inverse.XPath
{
    internal static class AnnotationServiceExtensions
    {
        internal static void ClearMatches(this AnnotationService service)
        {
            foreach (var annotation in service.Store.GetAnnotations())
            {
                service.Store.DeleteAnnotation(annotation.Id);
            }
        }
    }
}