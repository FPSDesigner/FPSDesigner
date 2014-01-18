/*
 * SkinnedModelContent.cs
 * Author: Bruno Evangelista
 * Copyright (c) 2008 Bruno Evangelista. All rights reserved.
 *
 * THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace XNAnimationPipeline
{
    public class SkinnedModelContent
    {
        [ContentSerializer(ElementName = "Model")]
        private readonly ModelContent model;

        [ContentSerializer(ElementName = "Skeleton")]
        private readonly SkinnedModelBoneContentCollection skeleton;

        [ContentSerializer(ElementName = "AnimationClipDictionary")]
        private readonly AnimationClipContentDictionary animationClips;

        internal SkinnedModelContent(ModelContent model, SkinnedModelBoneContentCollection skeleton,
            AnimationClipContentDictionary animationClips)
        {
            this.model = model;
            this.skeleton = skeleton;
            this.animationClips = animationClips;
        }

        internal void Write(ContentWriter output)
        {
            output.WriteObject<ModelContent>(model);
            WriteBones(output);
            WriteAnimationClips(output);
        }

        private void WriteBones(ContentWriter output)
        {
            output.Write(skeleton.Count);
            foreach (SkinnedModelBoneContent bone in skeleton)
                //{
                output.WriteSharedResource(bone);
            /*
                output.Write(skeleton[i].Name);
                output.Write(skeleton[i].BindPoseTransform);
                output.Write(skeleton[i].InverseBindPoseTransform);

                output.WriteSharedResource(skeleton[i].Parent);

                // Write children
                output.Write(skeleton[i].Children.Count);
                for (int j = 0; j < skeleton.Count; j++)
                {
                    output.WriteSharedResource(skeleton[i].Children[j]);
                }
                 */
            //}
            /*
            // Write root bone
            output.Write(rootBoneContent.Name);

            // Write each bone
            output.Write(skinnedModelBoneContentDictionary.Values.Count);
            foreach (SkinnedModelBoneContent bone in skinnedModelBoneContentDictionary.Values)
            {
                output.Write(bone.Name);
                output.Write(bone.Transform);
                output.Write(bone.AbsoluteTransform);
                output.Write(bone.InverseBindPoseTransform);
            }

            // Write the parent and children of each bone
            foreach (SkinnedModelBoneContent bone in skinnedModelBoneContentDictionary.Values)
            {
                string parentBoneName = null;
                if (bone.Parent != null)
                    parentBoneName = bone.Parent.Name;
                output.WriteObject<string>(parentBoneName);

                // Children
                output.Write(bone.Children.Count);
                foreach (SkinnedModelBoneContent childBbone in bone.Children)
                    output.Write(childBbone.Name);
            }
             */
        }

        private void WriteAnimationClips(ContentWriter output)
        {
            output.Write(animationClips.Count);
            foreach (AnimationClipContent animationClipContent in animationClips.Values)
                output.WriteSharedResource(animationClipContent);

            /*
            foreach (AnimationClipContent animationTrack in animationClips.Values)
            {
                output.Write(animationTrack.Name);
                output.WriteObject<TimeSpan>(animationTrack.Duration);

                output.Write(animationTrack.Channels.Count);
                foreach (KeyValuePair<string, AnimationChannelContent> pairKeyChannel in
                    animationTrack.Channels)
                {
                    output.Write(pairKeyChannel.Key);
                    output.Write(pairKeyChannel.Value.Count);
                    foreach (AnimationKeyframeContent keyframe in pairKeyChannel.Value)
                    {
                        output.WriteObject<TimeSpan>(keyframe.Time);
                        output.Write(keyframe.Transform);
                    }
                }
            }
             */
        }
    }
}