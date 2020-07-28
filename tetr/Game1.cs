﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace tetrIZ
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	/// 

	struct Point
	{
		public int i;
		public int j;

		public Point(int v1, int v2) : this()
		{
			this.i = v1;
			this.j = v2;
		}
	}

	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		int[][] field;
		Texture2D txtr;
		Texture2D act;
		Texture2D death;
		int n = 20;
		int m = 10;
		int tile_size = 32;
		int screen_w = 800;
		int screen_h = 800;
		Texture2D blackRectangle;
		int[][] figures;
		Point[] p1;
		int dx = 0;
		bool gen = true;
		float timeSinceLastFall = 0f;
		KeyboardState prevKeyState;
		KeyboardState curKeyState;
		Random rnd = new Random();
		char cur_fig;
		bool gameover = false;
		int lcnt = 0;
		int rcnt = 0;
		int dcnt = 0;
		int score = 0;
		bool hd = false;
		bool fl = false;
		SpriteFont font;
		bool menu = true;
		int menu_ptr = 0;
		
		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			//graphics.SynchronizeWithVerticalRetrace = false;
			Content.RootDirectory = "Content";
		}
		protected override void Initialize()
		{
			base.Initialize();
			field = new int[n][];
			for (int i = 0; i < n; i++)
			{
				field[i] = new int[m];
				for (int j = 0; j < m; j++) field[i][j] = 0;
			}
			figures = new[]
			{
				new[]{ 1,3,5,7 }, // I
				new[]{ 2,4,5,7 }, // Z
				new[]{ 3,5,4,6 }, // S
				new[]{ 3,5,4,7 }, // T
				new[]{ 2,3,5,7 }, // L
				new[]{ 3,5,7,6 }, // J
				new[]{ 2,3,4,5 }  // O
			};
			p1 = new Point[4];
			blackRectangle = new Texture2D(GraphicsDevice, 1, 1);
			blackRectangle.SetData(new[] { Color.Black });
			graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
			graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
			graphics.IsFullScreen = true;
			graphics.ApplyChanges();
			spriteBatch = new SpriteBatch(GraphicsDevice);
		}
		protected override void LoadContent()
		{
			txtr = this.Content.Load<Texture2D>("tetr");
			act = this.Content.Load<Texture2D>("active");
			death = this.Content.Load<Texture2D>("death");
			font = this.Content.Load<SpriteFont>("score");
		}
		protected override void UnloadContent()
		{

		}

		private bool check()
		{
			for (int i = 0; i < 4; i++)
			{
				if (p1[i].i < 0 || p1[i].i >= n || p1[i].j < 0 || p1[i].j >= m || field[p1[i].i][p1[i].j] != 0) return false;
			}
			return true;
		}

		private void UpdateScore(int add_cnt)
		{
			int mlp = 1 + (hd ? 1 : 0) + (fl ? 1 : 0);
			if (add_cnt == 1) score += 100 * mlp;
			if (add_cnt == 2) score += 300 * mlp;
			if (add_cnt == 3) score += 700 * mlp;
			if (add_cnt == 4) score += 1500 * mlp;
		}

		private void GenerateFigure()
		{
			gen = false;
			int k = rnd.Next(0, 7);
			if (k == 0) cur_fig = 'I';
			if (k == 1) cur_fig = 'Z';
			if (k == 2) cur_fig = 'S';
			if (k == 3) cur_fig = 'T';
			if (k == 4) cur_fig = 'L';
			if (k == 5) cur_fig = 'J';
			if (k == 6) cur_fig = 'O';
			for (int i = 0; i < 4; i++) p1[i] = new Point(figures[k][i] % 2, 3 + figures[k][i] / 2);
		}

		private void RotateCurrentFigure()
		{
			if (cur_fig == 'O') return;
			Point p = p1[1];
			if (cur_fig == 'L') p = p1[2];
			var b = new Point[4];
			p1.CopyTo(b, 0);
			for (int i = 0; i < 4; i++)
			{
				int x = p1[i].i - p.i;
				int y = p1[i].j - p.j;
				p1[i].j = p.j - x;
				p1[i].i = p.i + y;
			}
			if (!check()) for (int i = 0; i < 4; i++) p1[i] = b[i];
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || curKeyState.IsKeyDown(Keys.Escape))
				Exit();
			prevKeyState = curKeyState;
			curKeyState = Keyboard.GetState();
			if (menu)
			{
				
				if (curKeyState.IsKeyDown(Keys.Down) && !prevKeyState.IsKeyDown(Keys.Down)) menu_ptr = (menu_ptr + 1) % 3;
				if (curKeyState.IsKeyDown(Keys.Up) && !prevKeyState.IsKeyDown(Keys.Up))
				{
					menu_ptr--;
					if (menu_ptr == -1) menu_ptr = 2;
				}
				if (curKeyState.IsKeyDown(Keys.Space) && !prevKeyState.IsKeyDown(Keys.Space))
				{
					if (menu_ptr == 0)
					{
						menu = false;
					}
					if (menu_ptr == 1)
					{
						hd = !hd;
					}
					if (menu_ptr == 2)
					{
						fl = !fl;
					}
				}
				return;
			}
			if (gameover)
			{
				if (curKeyState.IsKeyDown(Keys.R) && !prevKeyState.IsKeyDown(Keys.R))
				{
					gameover = false;
					menu = true;
					menu_ptr = 0;
					field = new int[n][];
					for (int i = 0; i < n; i++)
					{
						field[i] = new int[m];
						for (int j = 0; j < m; j++) field[i][j] = 0;
					}
					score = 0;
				}
				base.Update(gameTime);
				return;
			}
			int[][] newf = new int[n][];
			for (int i = 0; i < n; i++) newf[i] = new int[m];
			int pk = n - 1;
			int add_cnt = 0;
			for (int i = n - 1; i >= 0; i--)
			{
				int cnt = 0;
				for (int j = 0; j < m; j++)
				{
					cnt += field[i][j];
				}
				if (cnt != m)
				{
					for (int j = 0; j < m; j++) newf[pk][j] = field[i][j];
					pk--;
				}
				else
				{
					add_cnt++;
				}
			}
			for (int i = 0; i < n; i++) for (int j = 0; j < m; j++) field[i][j] = newf[i][j];
			dx = 0;
			bool rotate = false;
			bool down = false;
			bool slowdown = false;
			UpdateScore(add_cnt);
			timeSinceLastFall += (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (gen)
			{
				GenerateFigure();
			}
			int a = score / 5000;
			if (timeSinceLastFall > Math.Max(0.05f, 0.75f - a * 0.1))
			{
				for (int i = 0; i < 4; i++) p1[i].i++;
				timeSinceLastFall = 0f;
			}
			bool f = false;
			for (int i = 0; i < 4; i++)
			{
				if (p1[i].i == n || field[p1[i].i][p1[i].j] != 0)
				{
					f = true;
				}
			}
			if (f)
			{
				gen = true;
				for (int i = 0; i < 4; i++)
				{
					p1[i].i--;
					if (p1[i].i <= -1)
					{
						gameover = true;
						return;
					}
					field[p1[i].i][p1[i].j] = 1;

				}
			}
			if (!curKeyState.IsKeyDown(Keys.Right)) rcnt = 1;
			if (!curKeyState.IsKeyDown(Keys.Left)) lcnt = 1;
			if (!curKeyState.IsKeyDown(Keys.Down)) dcnt = 1;
			if (curKeyState.IsKeyDown(Keys.Right) && prevKeyState.IsKeyDown(Keys.Right)) rcnt++;
			if (curKeyState.IsKeyDown(Keys.Left) && prevKeyState.IsKeyDown(Keys.Left)) lcnt++;
			if (curKeyState.IsKeyDown(Keys.Up) && !prevKeyState.IsKeyDown(Keys.Up)) down = true;
			if (curKeyState.IsKeyDown(Keys.Space) && !prevKeyState.IsKeyDown(Keys.Space)) rotate = true;
			if (curKeyState.IsKeyDown(Keys.Down) && prevKeyState.IsKeyDown(Keys.Down)) dcnt++;
			if (lcnt % 3 == 0) dx = -1;
			if (rcnt % 3 == 0) dx = 1;
			if (dcnt % 3 == 0) slowdown = true;
			if (slowdown)
			{
				for (int i = 0; i < 4; i++) p1[i].i++;
				if (!check()) for (int i = 0; i < 4; i++) p1[i].i--;
			}
			if (dx != 0)
			{
				for (int i = 0; i < 4; i++) p1[i].j += dx;
				if (!check()) for (int i = 0; i < 4; i++) p1[i].j -= dx;
				return;
			}
			if (rotate)
			{
				RotateCurrentFigure();
				return;
			}
			if (down)
			{
				while (check())
				{
					for (int i = 0; i < 4; i++)
					{
						p1[i].i++;
					}
				}
				for (int i = 0; i < 4; i++)
				{
					p1[i].i--;
					if (p1[i].i <= -1)
					{
						gameover = true;
						return;
					}
					field[p1[i].i][p1[i].j] = 1;
				}
				gen = true;
				return;
			}
			base.Update(gameTime);
		}

		private static int Distance(int x1, int y1, int x, int y)
		{
			return (int)Math.Ceiling(Math.Sqrt((x1 - x) * (x1 - x) + (y - y1) * (y - y1)));
		}

		private RenderTarget2D GenerateScreen()
		{
			RenderTarget2D mainRender = new RenderTarget2D(graphics.GraphicsDevice, 800, 800, false,
				GraphicsDevice.PresentationParameters.BackBufferFormat,
				DepthFormat.Depth24);
			GraphicsDevice.SetRenderTarget(mainRender);
			GraphicsDevice.Clear(Color.DimGray);
			spriteBatch.Begin();
			if (menu)
			{
				spriteBatch.DrawString(font, "START", new Vector2(200, 150), Color.White);
				spriteBatch.DrawString(font, "HD", new Vector2(200, 200), (hd ? Color.Yellow : Color.White));
				spriteBatch.DrawString(font, "FL", new Vector2(200, 250), (fl ? Color.Yellow : Color.White));
				spriteBatch.Draw(blackRectangle, new Rectangle(180, 160 + menu_ptr * 50, 20, 20), Color.White);
			}
			else if (gameover)
			{
				spriteBatch.Draw(death, new Vector2(50, 50));
				spriteBatch.DrawString(font, "PRESS R TO RESTART", new Vector2(50, 300), Color.White);
			}
			else
			{
				for (int i = 0; i < n; i++)
				{
					for (int j = 0; j < m; j++)
					{
						if (field[i][j] != 0)
						{
							spriteBatch.Draw(txtr, new Rectangle(100 + j * tile_size, screen_h - (n - i) * tile_size - 50, tile_size, tile_size), Color.White);
							if (fl)
							{
								spriteBatch.Draw(blackRectangle, new Rectangle(100 + j * tile_size, screen_h - (n - i) * tile_size - 50, tile_size, tile_size), Color.Black * (Distance(i, j, p1[0].i, p1[0].j) / 10f));
							}
						}
						else spriteBatch.Draw(blackRectangle, new Rectangle(100 + j * tile_size, screen_h - (n - i) * tile_size - 50, tile_size, tile_size), Color.White);

					}
				}
				foreach (var p in p1)
				{
					if (hd)
					{
						spriteBatch.Draw(act, new Rectangle(100 + p.j * tile_size, screen_h - (n - p.i) * tile_size - 50, tile_size, tile_size), Color.White * (1 / (1f * (p.i + 1) * 5)));
					}
					else
					{
						spriteBatch.Draw(act, new Rectangle(100 + p.j * tile_size, screen_h - (n - p.i) * tile_size - 50, tile_size, tile_size), Color.White);
					}
				}
			}
			spriteBatch.DrawString(font, "score: " + score.ToString(), new Vector2(450, 150), Color.White);
			spriteBatch.End();
			return mainRender;
		}


		protected override void Draw(GameTime gameTime)
		{
			var toDraw = GenerateScreen();
			GraphicsDevice.SetRenderTarget(null);
			spriteBatch.Begin();
			spriteBatch.Draw(toDraw, new Rectangle((1920 - 1080) / 2, 0, 1080, 1080), Color.White);
			spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
