/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Spine {
	public class SkeletonRenderer {
		GraphicsDevice device;
		public SpriteBatcher batcher;
		BasicEffect effect;
		RasterizerState rasterizerState;
		public BlendState BlendState { get; set; }
		float[] vertices = new float[8];

		public SkeletonRenderer (GraphicsDevice device) {
			this.device = device;

			batcher = new SpriteBatcher();

			effect = new BasicEffect(device);
			effect.World = Matrix.Identity;
			effect.View = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 1.0f), Vector3.Zero, Vector3.Up);
			effect.TextureEnabled = true;
			effect.VertexColorEnabled = true;

			rasterizerState = new RasterizerState();
			rasterizerState.CullMode = CullMode.None;

			BlendState = BlendState.NonPremultiplied;

			Bone.yDown = true;
		}

        public void setCameraMatrix(Matrix camera)
        {
            effect.View = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 1.0f), Vector3.Zero, Vector3.Up) * camera;
        }

		public void Begin () {
			device.RasterizerState = rasterizerState;
			device.BlendState = BlendState;

			effect.Projection = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, device.Viewport.Height, 0, 1, 0);
		}

		public void End () {
			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Apply();
				batcher.Draw(device);
			}
		}

		public void Draw (Skeleton skeleton) {
			List<Slot> drawOrder = skeleton.DrawOrder;
			for (int i = 0, n = drawOrder.Count; i < n; i++) {
				Slot slot = drawOrder[i];
				RegionAttachment regionAttachment = slot.Attachment as RegionAttachment;
				if (regionAttachment != null) {
					SpriteBatchItem item = batcher.CreateBatchItem();
					item.Texture = (Texture2D)regionAttachment.RendererObject;

					byte r = (byte)(skeleton.R * slot.R * 255);
					byte g = (byte)(skeleton.G * slot.G * 255);
					byte b = (byte)(skeleton.B * slot.B * 255);
					byte a = (byte)(skeleton.A * slot.A * 255);
					item.vertexTL.Color.R = r;
					item.vertexTL.Color.G = g;
					item.vertexTL.Color.B = b;
					item.vertexTL.Color.A = a;
					item.vertexBL.Color.R = r;
					item.vertexBL.Color.G = g;
					item.vertexBL.Color.B = b;
					item.vertexBL.Color.A = a;
					item.vertexBR.Color.R = r;
					item.vertexBR.Color.G = g;
					item.vertexBR.Color.B = b;
					item.vertexBR.Color.A = a;
					item.vertexTR.Color.R = r;
					item.vertexTR.Color.G = g;
					item.vertexTR.Color.B = b;
					item.vertexTR.Color.A = a;

					float[] vertices = this.vertices;
					regionAttachment.ComputeVertices(slot.Bone, vertices);
					item.vertexTL.Position.X = vertices[RegionAttachment.X1];
					item.vertexTL.Position.Y = vertices[RegionAttachment.Y1];
					item.vertexTL.Position.Z = 0;
					item.vertexBL.Position.X = vertices[RegionAttachment.X2];
					item.vertexBL.Position.Y = vertices[RegionAttachment.Y2];
					item.vertexBL.Position.Z = 0;
					item.vertexBR.Position.X = vertices[RegionAttachment.X3];
					item.vertexBR.Position.Y = vertices[RegionAttachment.Y3];
					item.vertexBR.Position.Z = 0;
					item.vertexTR.Position.X = vertices[RegionAttachment.X4];
					item.vertexTR.Position.Y = vertices[RegionAttachment.Y4];
					item.vertexTR.Position.Z = 0;

					float[] uvs = regionAttachment.UVs;
					item.vertexTL.TextureCoordinate.X = uvs[RegionAttachment.X1];
					item.vertexTL.TextureCoordinate.Y = uvs[RegionAttachment.Y1];
					item.vertexBL.TextureCoordinate.X = uvs[RegionAttachment.X2];
					item.vertexBL.TextureCoordinate.Y = uvs[RegionAttachment.Y2];
					item.vertexBR.TextureCoordinate.X = uvs[RegionAttachment.X3];
					item.vertexBR.TextureCoordinate.Y = uvs[RegionAttachment.Y3];
					item.vertexTR.TextureCoordinate.X = uvs[RegionAttachment.X4];
					item.vertexTR.TextureCoordinate.Y = uvs[RegionAttachment.Y4];
				}
			}
		}



        /// <summary>
        /// A test method written by Dan. This is to sneak PattyPetitGiant's draw calls onto Spine's VertexArray.
        /// </summary>
        /// <param name="texture">Source texture.</param>
        /// <param name="srcRectangle">Source rectangle.</param>
        /// <param name="dstPosition">Destination location</param>
        /// <param name="color">NONFUNCTIONAL: Color tint.</param>
        /// <param name="rotation">Rotation around center of graphic to draw.</param>
        /// <param name="scale">Scale from centerpoint of graphic to draw.</param>
        public void DrawSpriteToSpineVertexArray(Texture2D texture, Rectangle srcRectangle, Vector2 dstPosition, Color color, float rotation, Vector2 scale)
        {
            Rectangle dstRectangle = new Rectangle((int)dstPosition.X, (int)dstPosition.Y, srcRectangle.Width + 1, srcRectangle.Height + 1);

            SpriteBatchItem item = batcher.CreateBatchItem();
            item.Texture = texture;

            //set wall colors
            item.vertexTL.Color = color;
            item.vertexBL.Color = color;
            item.vertexBR.Color = color;
            item.vertexTR.Color = color;

            item.vertexTL.Position.X = dstRectangle.Left;
            item.vertexTL.Position.Y = dstRectangle.Top;
            item.vertexTL.Position.Z = 0;
            item.vertexBL.Position.X = dstRectangle.Left;
            item.vertexBL.Position.Y = dstRectangle.Bottom;
            item.vertexBL.Position.Z = 0;
            item.vertexBR.Position.X = dstRectangle.Right;
            item.vertexBR.Position.Y = dstRectangle.Bottom;
            item.vertexBR.Position.Z = 0;
            item.vertexTR.Position.X = dstRectangle.Right;
            item.vertexTR.Position.Y = dstRectangle.Top;
            item.vertexTR.Position.Z = 0;

            item.vertexTL.TextureCoordinate = GetUV(texture, srcRectangle.Left, srcRectangle.Top);
            item.vertexBL.TextureCoordinate = GetUV(texture, srcRectangle.Left, srcRectangle.Bottom);
            item.vertexBR.TextureCoordinate = GetUV(texture, srcRectangle.Right, srcRectangle.Bottom);
            item.vertexTR.TextureCoordinate = GetUV(texture, srcRectangle.Right, srcRectangle.Top);

            Matrix world = Matrix.CreateTranslation(((srcRectangle.Width / 2) + dstRectangle.X) * -1, ((srcRectangle.Height / 2) + dstRectangle.Y) * -1, 0) * Matrix.CreateRotationZ(rotation) * Matrix.CreateScale(scale.X, scale.Y, 0.0f) * Matrix.CreateTranslation(((srcRectangle.Width / 2) + dstRectangle.X), ((srcRectangle.Height / 2) + dstRectangle.Y), 0) * effect.World;
            Vector3.Transform(ref item.vertexTL.Position, ref world, out item.vertexTL.Position);
            Vector3.Transform(ref item.vertexBL.Position, ref world, out item.vertexBL.Position);
            Vector3.Transform(ref item.vertexBR.Position, ref world, out item.vertexBR.Position);
            Vector3.Transform(ref item.vertexTR.Position, ref world, out item.vertexTR.Position);
        }

        Vector2 GetUV(Texture2D tex, float x, float y)
        {
            return new Vector2(x / (float)tex.Width, y / (float)tex.Height);
        }
	}
}
