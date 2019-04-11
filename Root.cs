using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGame
{
    class Root : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private BoardGA m_boardGA;
        private bool    m_paused;
        private bool    m_previousSpacePressed;
        public Root()
        {
            graphics = new GraphicsDeviceManager(this);

            // Window
            graphics.PreferredBackBufferWidth = 960;
            graphics.PreferredBackBufferHeight = 540;

            // Fullscreen Window (Max Resolution)
            //graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            //graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            // Fullscreen (Max Resolution)
            //graphics.IsFullScreen = true;

            graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Log.Clear();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            Log.Print("Terminal Active\n");
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            m_boardGA = new BoardGA(Defines.POP_SIZE, Defines.CROSSOVER_RATE, Defines.MUTATION_RATE, Defines.BOARD_WIDTH, Content);
            m_paused = false;
            m_previousSpacePressed = false;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            bool isSpaceKey = Keyboard.GetState().IsKeyDown(Keys.Space);

            if(!m_previousSpacePressed)
            {
                if(isSpaceKey)
                {
                    m_paused = !m_paused;
                    m_previousSpacePressed = true;
                }
            }
            else
            {
                if(!isSpaceKey)
                {
                    m_previousSpacePressed = false;
                }
            }

            if(!m_paused)
            {
                m_boardGA.Run(Defines.POP_SIZE);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            m_boardGA.Render(spriteBatch, 
                             graphics.PreferredBackBufferWidth, 
                             graphics.PreferredBackBufferHeight);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
