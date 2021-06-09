using ThunderKit.Core.Pipelines;

namespace Scripts
{
    [PipelineSupport(typeof(Pipeline))]
    public class  : PipelineJob
    {
        public override void Execute(Pipeline pipeline)
        {
        }
    }
}
