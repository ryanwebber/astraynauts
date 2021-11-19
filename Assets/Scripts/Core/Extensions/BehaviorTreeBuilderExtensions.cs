using CleverCrow.Fluid.BTs.TaskParents.Composites;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using UnityEngine;

public interface IBehaviorTreeBuildable
{
    BehaviorTree ToBehaviorTree(GameObject obj);
}

public static class BehaviorTreeBuilderExtensions
{
    public class ShortCircuitSequence : CompositeBase
    {
        private System.Func<bool> condition;

        public ShortCircuitSequence(string name, System.Func<bool> condition)
        {
            this.Name = name;
            this.condition = condition;
        }

        protected override TaskStatus OnUpdate()
        {
            if (condition?.Invoke() ?? false)
                return TaskStatus.Failure;

            for (var i = ChildIndex; i < Children.Count; i++)
            {
                var child = Children[ChildIndex];

                var status = child.Update();
                if (status != TaskStatus.Success)
                {
                    return status;
                }

                ChildIndex++;
            }

            return TaskStatus.Success;
        }
    }

    public static BehaviorTreeBuilder Splice(this BehaviorTreeBuilder builder, IBehaviorTreeBuildable buildable)
    {
        return builder.Splice(buildable.ToBehaviorTree(builder.Owner));
    }

    public static BehaviorTreeBuilder Success(this BehaviorTreeBuilder builder, System.Action action)
    {
        return builder.Do(() =>
        {
            action?.Invoke();
            return TaskStatus.Success;
        });
    }

    public static BehaviorTreeBuilder ShortCircuit(this BehaviorTreeBuilder builder, string name, System.Func<bool> condition)
    {
        return builder.AddNodeWithPointer(new ShortCircuitSequence(name, condition));
    }

    public static BehaviorTreeBuilder WaitUntil(this BehaviorTreeBuilder builder, System.Func<bool> condition)
    {
        return builder
            .RepeatUntilSuccess()
                .Condition(condition)
            .End();
    }
}
