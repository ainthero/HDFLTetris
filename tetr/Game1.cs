using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace tetrIZ;

/// <summary>
/// This is the main type for your game.
/// </summary>
/// 
internal struct Point
{
    public int I;
    public int J;

    public Point(int v1, int v2) : this()
    {
        I = v1;
        J = v2;
    }
}

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private int[][] _field;
    private Texture2D _txtr;
    private Texture2D _act;
    private Texture2D _death;
    private const int N = 20;
    private const int M = 10;
    private const int TileSize = 32;
    private const int ScreenH = 800;
    private Texture2D _blackRectangle;
    private int[][] _figures;
    private Point[] _p1;
    private int _dx;
    private bool _gen = true;
    private float _timeSinceLastFall;
    private KeyboardState _prevKeyState;
    private KeyboardState _curKeyState;
    private readonly Random _rnd = new();
    private char _curFig;
    private bool _gameover;
    private int _lcnt;
    private int _rcnt;
    private int _dcnt;
    private int _score;
    private bool _hd;
    private bool _fl;
    private SpriteFont _font;
    private bool _menu = true;
    private int _menuPtr;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        //graphics.SynchronizeWithVerticalRetrace = false;
        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        base.Initialize();
        _field = new int[N][];
        for (var i = 0; i < N; i++)
        {
            _field[i] = new int[M];
            for (var j = 0; j < M; j++) _field[i][j] = 0;
        }

        _figures = new[]
        {
            new[] {1, 3, 5, 7}, // I
            new[] {2, 4, 5, 7}, // Z
            new[] {3, 5, 4, 6}, // S
            new[] {3, 5, 4, 7}, // T
            new[] {2, 3, 5, 7}, // L
            new[] {3, 5, 7, 6}, // J
            new[] {2, 3, 4, 5} // O
        };
        _p1 = new Point[4];
        _blackRectangle = new Texture2D(GraphicsDevice, 1, 1);
        _blackRectangle.SetData(new[] {Color.Black});
        _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void LoadContent()
    {
        _txtr = Content.Load<Texture2D>("tetr");
        _act = Content.Load<Texture2D>("active");
        _death = Content.Load<Texture2D>("death");
        _font = Content.Load<SpriteFont>("score");
    }

    protected override void UnloadContent()
    {
    }

    private bool Check()
    {
        for (var i = 0; i < 4; i++)
        {
            if (_p1[i].I < 0 || _p1[i].I >= N || _p1[i].J < 0 || _p1[i].J >= M ||
                _field[_p1[i].I][_p1[i].J] != 0) return false;
        }

        return true;
    }

    private void UpdateScore(int addCnt)
    {
        var mlp = 1 + (_hd ? 1 : 0) + (_fl ? 1 : 0);
        _score += addCnt switch
        {
            1 => 100 * mlp,
            2 => 300 * mlp,
            3 => 700 * mlp,
            4 => 1500 * mlp,
            _ => throw new ArgumentOutOfRangeException(nameof(addCnt), addCnt, null)
        };
    }

    private void GenerateFigure()
    {
        _gen = false;
        var k = _rnd.Next(0, 7);
        _curFig = k switch
        {
            0 => 'I',
            1 => 'Z',
            2 => 'S',
            3 => 'T',
            4 => 'L',
            5 => 'J',
            6 => 'O',
            _ => _curFig
        };
        for (var i = 0; i < 4; i++)
            _p1[i] = new Point(_figures[k][i] % 2, 3 + _figures[k][i] / 2);
    }

    private void RotateCurrentFigure()
    {
        if (_curFig == 'O')
            return;

        var p = _p1[1];
        if (_curFig == 'L')
            p = _p1[2];

        var b = new Point[4];
        _p1.CopyTo(b, 0);
        for (var i = 0; i < 4; i++)
        {
            var x = _p1[i].I - p.I;
            var y = _p1[i].J - p.J;
            _p1[i].J = p.J - x;
            _p1[i].I = p.I + y;
        }

        if (!Check())
        {
            for (var i = 0; i < 4; i++)
                _p1[i] = b[i];
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            _curKeyState.IsKeyDown(Keys.Escape))
            Exit();

        _prevKeyState = _curKeyState;
        _curKeyState = Keyboard.GetState();
        if (_menu)
        {
            if (_curKeyState.IsKeyDown(Keys.Down) && !_prevKeyState.IsKeyDown(Keys.Down)) _menuPtr = (_menuPtr + 1) % 3;
            if (_curKeyState.IsKeyDown(Keys.Up) && !_prevKeyState.IsKeyDown(Keys.Up))
            {
                _menuPtr--;
                if (_menuPtr == -1) _menuPtr = 2;
            }

            if (_curKeyState.IsKeyDown(Keys.Space) && !_prevKeyState.IsKeyDown(Keys.Space))
            {
                switch (_menuPtr)
                {
                    case 0:
                        _menu = false;
                        break;
                    case 1:
                        _hd = !_hd;
                        break;
                    case 2:
                        _fl = !_fl;
                        break;
                }
            }

            return;
        }

        if (_gameover)
        {
            if (_curKeyState.IsKeyDown(Keys.R) && !_prevKeyState.IsKeyDown(Keys.R))
            {
                _gameover = false;
                _menu = true;
                _menuPtr = 0;
                _field = new int[N][];
                for (var i = 0; i < N; i++)
                {
                    _field[i] = new int[M];
                    for (var j = 0; j < M; j++) _field[i][j] = 0;
                }

                _score = 0;
            }

            base.Update(gameTime);
            return;
        }

        var newf = new int[N][];
        for (var i = 0; i < N; i++) newf[i] = new int[M];
        var pk = N - 1;
        var addCnt = 0;
        for (var i = N - 1; i >= 0; i--)
        {
            var cnt = 0;
            for (var j = 0; j < M; j++)
            {
                cnt += _field[i][j];
            }

            if (cnt != M)
            {
                for (var j = 0; j < M; j++) newf[pk][j] = _field[i][j];
                pk--;
            }
            else
            {
                addCnt++;
            }
        }

        for (var i = 0; i < N; i++)
        for (var j = 0; j < M; j++)
            _field[i][j] = newf[i][j];
        _dx = 0;
        var rotate = false;
        var down = false;
        var slowdown = false;
        UpdateScore(addCnt);
        _timeSinceLastFall += (float) gameTime.ElapsedGameTime.TotalSeconds;
        if (_gen)
        {
            GenerateFigure();
        }

        var a = _score / 5000;
        if (_timeSinceLastFall > Math.Max(0.05f, 0.75f - a * 0.1))
        {
            for (var i = 0; i < 4; i++) _p1[i].I++;
            _timeSinceLastFall = 0f;
        }

        var f = false;
        for (var i = 0; i < 4; i++)
        {
            if (_p1[i].I == N || _field[_p1[i].I][_p1[i].J] != 0)
            {
                f = true;
            }
        }

        if (f)
        {
            _gen = true;
            for (var i = 0; i < 4; i++)
            {
                _p1[i].I--;
                if (_p1[i].I <= -1)
                {
                    _gameover = true;
                    return;
                }

                _field[_p1[i].I][_p1[i].J] = 1;
            }
        }

        if (!_curKeyState.IsKeyDown(Keys.Right)) _rcnt = 1;
        if (!_curKeyState.IsKeyDown(Keys.Left)) _lcnt = 1;
        if (!_curKeyState.IsKeyDown(Keys.Down)) _dcnt = 1;
        if (_curKeyState.IsKeyDown(Keys.Right) && _prevKeyState.IsKeyDown(Keys.Right)) _rcnt++;
        if (_curKeyState.IsKeyDown(Keys.Left) && _prevKeyState.IsKeyDown(Keys.Left)) _lcnt++;
        if (_curKeyState.IsKeyDown(Keys.Up) && !_prevKeyState.IsKeyDown(Keys.Up)) down = true;
        if (_curKeyState.IsKeyDown(Keys.Space) && !_prevKeyState.IsKeyDown(Keys.Space)) rotate = true;
        if (_curKeyState.IsKeyDown(Keys.Down) && _prevKeyState.IsKeyDown(Keys.Down)) _dcnt++;
        if (_lcnt % 3 == 0) _dx = -1;
        if (_rcnt % 3 == 0) _dx = 1;
        if (_dcnt % 3 == 0) slowdown = true;
        if (slowdown)
        {
            for (var i = 0; i < 4; i++) _p1[i].I++;
            if (!Check())
                for (var i = 0; i < 4; i++)
                    _p1[i].I--;
        }

        if (_dx != 0)
        {
            for (var i = 0; i < 4; i++) _p1[i].J += _dx;
            if (!Check())
                for (var i = 0; i < 4; i++)
                    _p1[i].J -= _dx;
            return;
        }

        if (rotate)
        {
            RotateCurrentFigure();
            return;
        }

        if (down)
        {
            while (Check())
            {
                for (var i = 0; i < 4; i++)
                {
                    _p1[i].I++;
                }
            }

            for (var i = 0; i < 4; i++)
            {
                _p1[i].I--;
                if (_p1[i].I <= -1)
                {
                    _gameover = true;
                    return;
                }

                _field[_p1[i].I][_p1[i].J] = 1;
            }

            _gen = true;
            return;
        }

        base.Update(gameTime);
    }

    private static int Distance(int x1, int y1, int x, int y)
    {
        return (int) Math.Ceiling(Math.Sqrt((x1 - x) * (x1 - x) + (y - y1) * (y - y1)));
    }

    private RenderTarget2D GenerateScreen()
    {
        var mainRender = new RenderTarget2D(_graphics.GraphicsDevice, 800, 800, false,
            GraphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24);
        GraphicsDevice.SetRenderTarget(mainRender);
        GraphicsDevice.Clear(Color.DimGray);
        _spriteBatch.Begin();
        if (_menu)
        {
            _spriteBatch.DrawString(_font, "START", new Vector2(200, 150), Color.White);
            _spriteBatch.DrawString(_font, "HD", new Vector2(200, 200), (_hd ? Color.Yellow : Color.White));
            _spriteBatch.DrawString(_font, "FL", new Vector2(200, 250), (_fl ? Color.Yellow : Color.White));
            _spriteBatch.Draw(_blackRectangle, new Rectangle(180, 160 + _menuPtr * 50, 20, 20), Color.White);
        }
        else if (_gameover)
        {
            _spriteBatch.Draw(_death, new Vector2(50, 50), Color.White);
            _spriteBatch.DrawString(_font, "PRESS R TO RESTART", new Vector2(50, 300), Color.White);
        }
        else
        {
            for (var i = 0; i < N; i++)
            {
                for (var j = 0; j < M; j++)
                {
                    if (_field[i][j] != 0)
                    {
                        _spriteBatch.Draw(_txtr,
                            new Rectangle(100 + j * TileSize, ScreenH - (N - i) * TileSize - 50, TileSize, TileSize),
                            Color.White);
                        if (_fl)
                        {
                            _spriteBatch.Draw(_blackRectangle,
                                new Rectangle(100 + j * TileSize, ScreenH - (N - i) * TileSize - 50, TileSize,
                                    TileSize), Color.Black * (Distance(i, j, _p1[0].I, _p1[0].J) / 10f));
                        }
                    }
                    else
                        _spriteBatch.Draw(_blackRectangle,
                            new Rectangle(100 + j * TileSize, ScreenH - (N - i) * TileSize - 50, TileSize, TileSize),
                            Color.White);
                }
            }

            foreach (var p in _p1)
            {
                if (_hd)
                {
                    _spriteBatch.Draw(_act,
                        new Rectangle(100 + p.J * TileSize, ScreenH - (N - p.I) * TileSize - 50, TileSize, TileSize),
                        Color.White * (1 / (1f * (p.I + 1) * 5)));
                }
                else
                {
                    _spriteBatch.Draw(_act,
                        new Rectangle(100 + p.J * TileSize, ScreenH - (N - p.I) * TileSize - 50, TileSize, TileSize),
                        Color.White);
                }
            }
        }

        _spriteBatch.DrawString(_font, "score: " + _score.ToString(), new Vector2(450, 150), Color.White);
        _spriteBatch.End();
        return mainRender;
    }


    protected override void Draw(GameTime gameTime)
    {
        var toDraw = GenerateScreen();
        GraphicsDevice.SetRenderTarget(null);
        _spriteBatch.Begin();
        _spriteBatch.Draw(toDraw, new Rectangle((1920 - 1080) / 2, 0, 1080, 1080), Color.White);
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}