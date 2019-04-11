using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace MonoGame
{
    public struct CharNode
    {
        public char           character;
        public List<CharNode> children;
        public string         completeString;
    };

    public struct SolutionWord
    {
        public string word;
        public int    score;
    };

    public class Board
    {
        private string                   m_activeConfiguration;
        private int                      m_width;

        private SpriteFont               m_spriteFont;
        private SpriteFont               m_spriteFontMini;
        private Texture2D                m_squareTexture;

        CharNode                         m_trieTree;

        bool[,]                          m_busyDFS;
        Dictionary<string, bool>         m_alreadyBeenMap;

        public      Board(int width, string config, ContentManager content)
        {
            m_width = width;
            m_activeConfiguration = config;

            m_spriteFont = content.Load<SpriteFont>("VCR");
            m_spriteFontMini = content.Load<SpriteFont>("VCRMINI");
            m_squareTexture = content.Load<Texture2D>("Square");

            m_trieTree = new CharNode();
            m_trieTree.character = '.';
            m_trieTree.children = new List<CharNode>();

            m_busyDFS = new bool[m_width, m_width];

            for(int i = 0; i < m_width; i++)
            {
                for(int j = 0; j < m_width; j++)
                {
                    m_busyDFS[i, j] = false;
                }
            }

            m_alreadyBeenMap = new Dictionary<string, bool>();

            InitializeDictionary();

            List<SolutionWord> tmp = null;
            TestConfiguration(m_activeConfiguration, ref tmp);
        }

        public int  TestConfiguration(string config, ref List<SolutionWord> solution)
        {
            int finalScore = 0;
            char[,] charactersMatrix = new char[m_width, m_width];
            int     indexInString = 0;

            m_alreadyBeenMap.Clear();

            for(int i = 0; i < m_width; i++)
            {
                for(int j = 0; j < m_width; j++)
                {
                    charactersMatrix[i, j] = Char.ToLower(config[indexInString++]); 
                }
            }

            for(int i = 0; i < m_width; i++)
            {
                for(int j = 0; j < m_width; j++)
                {
                    DFS(i, j, ref charactersMatrix, m_trieTree, ref finalScore, ref solution);
                }
            }

            // For debugging purposes:
            // Console.WriteLine(finalScore);

            return finalScore;
        }

        public void SetActiveConfiguration(string config)
        {
            m_activeConfiguration = config;
        }

        public void Render(SpriteBatch spriteBatch, float screenWidth, float screenHeight)
        {
            string  drawableString = new string("");
            float   fontWidth = 60;
            float   fontHeight = 60;
            float   miniFontWidth = 20;
            float   miniFontHeight = 21;
            float boardTopY = screenHeight - 25.0f - (fontHeight * 4);
            float boardMostX = 25.0f;
            Vector2 pivotPosition = new Vector2(boardMostX, boardTopY);

            for(int i = 0; i < m_width; i++)
            {
                for(int j = 0; j < m_width; j++)
                {
                    drawableString += m_activeConfiguration[(i * m_width) + j].ToString() + " ";
                }
                if(i != m_width - 1)
                {
                    drawableString += "\n";
                }
            }

            for(int i = 0; i < m_width; i++)
            {
                for(int j = 0; j < m_width; j++)
                {
                    char crtChar = m_activeConfiguration[(i * m_width) + j];
                    Vector2 letterPosition = new Vector2(pivotPosition.X + j * fontWidth, pivotPosition.Y + i * fontHeight);
                    Rectangle currentRectangle = new Rectangle((int)(letterPosition.X - (fontWidth / 6.0f)), 
                                                               (int)letterPosition.Y, 
                                                               (int)fontWidth, 
                                                               (int)fontHeight);

                    spriteBatch.Draw(m_squareTexture, currentRectangle, null, Color.Orange, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
                    spriteBatch.DrawString((crtChar != '#') ? m_spriteFont : m_spriteFontMini, 
                                           (crtChar != '#') ? crtChar.ToString() : "Qu", 
                                           (crtChar != '#') ? letterPosition : new Vector2(letterPosition.X, letterPosition.Y + (miniFontWidth / 2.0f)), 
                                           Color.Cyan);
                }
            }

            List<SolutionWord> solution = new List<SolutionWord>();
            TestConfiguration(m_activeConfiguration, ref solution);

            float y = boardTopY;
            float x = boardMostX + m_width * (fontWidth);
            int maxWordLength = 0;

            float textSizeMulitplier = 45.0f / (float)solution.Count;

            foreach(SolutionWord solWord in solution)
            {
                string resultString = String.Format("{0}:{1}", solWord.word, solWord.score.ToString());
                spriteBatch.DrawString(m_spriteFontMini, resultString, new Vector2(x, y), Color.MediumSeaGreen, 0, Vector2.Zero, textSizeMulitplier, SpriteEffects.None, 0.0f);
                y += miniFontHeight * textSizeMulitplier;

                maxWordLength = Math.Max(maxWordLength, resultString.Length);

                if(y > screenHeight - (boardMostX * 1.25f))
                {
                    y = boardTopY;
                    x += (maxWordLength * miniFontWidth * textSizeMulitplier);
                    maxWordLength = 0;
                }
            }
        }

        private bool ValidPositionInBoard(int line, int column)
        {
            if(line >= 0   && line   < m_width &&
               column >= 0 && column < m_width)
            {
                return true;
            }

            return false;
        }

        private int  WordScore(int lgh)
        {
            switch(lgh)
            {
            case 0:
            case 1:
            case 2:
                return 0;
            case 3:
            case 4:
                return 1;
            case 5:
                return 2;
            case 6:
                return 3;
            case 7:
                return 5;
            default:
                return 11;
            }
        }

        private void DFS(int line, int column, ref char[,] charMatrix, CharNode crtNode, ref int score, ref List<SolutionWord> solution)
        {
            int[] dirY = new int[] {0, -1, 0, 1, -1, 1, -1, 1};
            int[] dirX = new int[] {1, 0, -1, 0, -1, 1, 1, -1};

            if(ValidPositionInBoard(line, column))
            {
                if(!m_busyDFS[line, column])
                {
                    CharNode nextNode = new CharNode();
                    nextNode.character = '~'; // Invalid character

                    if(charMatrix[line, column] != '#') // The Qu character
                    {
                        foreach(CharNode child in crtNode.children)
                            if(child.character == charMatrix[line, column])
                                nextNode = child;
                    }
                    else
                    {
                        foreach(CharNode child in crtNode.children)
                            if(child.character == 'q')
                                nextNode = child;
                        
                        if(nextNode.character != '~')
                        {
                            foreach(CharNode child in nextNode.children)
                                if(child.character == 'u')
                                    nextNode = child;
                        }
                        
                        if(nextNode.character == 'q')
                            nextNode.character = '~';
                    }
                    
                    if(nextNode.character != '~')
                    {
                        m_busyDFS[line, column] = true;

                        for(int i = 0; i < dirY.Length; i++)
                        {
                            int newLine = line + dirY[i];
                            int newColumn = column + dirX[i];

                            DFS(newLine, newColumn, ref charMatrix, nextNode, ref score, ref solution);
                        }

                        foreach(CharNode child in nextNode.children)
                        {
                            if(child.character == '_')
                            {
                                if(!m_alreadyBeenMap.ContainsKey(child.completeString))
                                {
                                    m_alreadyBeenMap[child.completeString] = true;
                                    int scr = WordScore(child.completeString.Length);

                                    score += scr;

                                    if(scr != 0 && solution != null)
                                    {
                                        SolutionWord solWord = new SolutionWord();
                                        solWord.word = child.completeString;
                                        solWord.score = scr;

                                        solution.Add(solWord);
                                    }

                                    // For debugging purposes: 
                                    // Console.WriteLine("{0} {1}", child.completeString, scr);
                                }
                            }
                        }

                        m_busyDFS[line, column] = false;
                    }
                }
            }
        }

        private void AddStringToTree(string str, int index, CharNode crtNode)
        {
            bool addedForward = false;
            foreach(var child in crtNode.children)
            {
                if(index < str.Length)
                {
                    if(child.character == Char.ToLower(str[index]))
                    {
                        AddStringToTree(str, index+1, child);
                        addedForward = true;
                    }
                }
                else
                {
                    if(child.character == '_')
                    {
                        addedForward = true;
                    }
                }
            }

            if(!addedForward)
            {
                if(index < str.Length)
                {
                    CharNode newNode = new CharNode();
                    newNode.character = Char.ToLower(str[index]);
                    newNode.children = new List<CharNode>();
                    crtNode.children.Add(newNode);

                    AddStringToTree(str, index + 1, newNode);
                }
                else
                {
                    CharNode newNode = new CharNode();
                    newNode.character = '_';
                    newNode.children = new List<CharNode>();
                    newNode.completeString = str;
                    crtNode.children.Add(newNode);
                }
            }
        }

        private void InitializeDictionary()
        {
            StreamReader sr = new StreamReader(Environment.CurrentDirectory + "/Content/BoggleDictionary.txt");
            string line = null;
            do
            {
                line = sr.ReadLine();

                if(line != null)
                {
                    AddStringToTree(line, 0, m_trieTree);
                }
            } while(line != null);

            sr.Close();
        }
    }
}