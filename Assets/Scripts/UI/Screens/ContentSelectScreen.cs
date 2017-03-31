﻿using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.UI.Screens
{
    // Class for content (expansions) selection page
    public class ContentSelectScreen
    {
        public Game game;
        // List of expansions selected by ID
        public List<string> selected;

        public Dictionary<string, List<TextButton>> buttons;

        // Create page
        public ContentSelectScreen()
        {
            // Clean everything up
            Destroyer.Destroy();
            game = Game.Get();

            // Find any content packs at the location
            game.cd = new ContentData(game.gameType.DataDirectory());
            // Check if we found anything (must have found at least base)
            if (game.cd.allPacks.Count == 0)
            {
                ValkyrieDebug.Log("Error: Failed to find any content packs, please check that you have them present in: " + game.gameType.DataDirectory() + System.Environment.NewLine);
                Application.Quit();
            }

            // Draw to the screen
            Draw();
        }

        // Draw the expansions on the screen, highlighting those that are enabled
        public void Draw()
        {
            // Clean up
            Destroyer.Dialog();
            // Initialise selected list
            selected = new List<string>();

            // Fetch which packs are selected from the current config (items under [Packs])
            Dictionary<string, string> setPacks = game.config.data.Get(game.gameType.TypeName() + "Packs");
            if (setPacks != null)
            {
                foreach (KeyValuePair<string, string> kv in setPacks)
                {
                    // As packs are just a list, only the key is used, value is empty
                    selected.Add(kv.Key);
                }
            }

            // Draw a header
            DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 2), "Select Expansion Content");
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.SetFont(game.gameType.GetHeaderFont());

            db = new DialogBox(new Vector2(1, 4f), new Vector2(UIScaler.GetWidthUnits()-2f, 22f), "");
            db.AddBorder();
            db.background.AddComponent<UnityEngine.UI.Mask>();
            UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

            GameObject scrollArea = new GameObject("scroll");
            RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
            scrollArea.transform.parent = db.background.transform;
            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);
            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (UIScaler.GetWidthUnits()-3f) * UIScaler.GetPixelsPerUnit());

            scrollRect.content = scrollInnerRect;
            scrollRect.horizontal = false;

            buttons = new Dictionary<string, List<TextButton>>();
            // Start here
            float offset = 4.5f;
            TextButton tb = null;
            bool left = true;
            // Note this is currently unordered
            foreach (ContentData.ContentPack cp in game.cd.allPacks)
            {
                // If the id is "" this is base content and can be ignored
                if (cp.id.Length > 0)
                {
                    string id = cp.id;
                    buttons.Add(id, new List<TextButton>());
                    Color bgColor = Color.white;
                    if (!selected.Contains(id))
                    {
                        bgColor = new Color(0.3f, 0.3f, 0.3f);
                    }

                    // Create a sprite with the pack's image
                    Texture2D tex = ContentData.FileToTexture(cp.image);
                    Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1);

                    if (left)
                    {
                        tb = new TextButton(new Vector2(2f, offset), new Vector2(6, 6), "", delegate { Select(id); });
                    }
                    else
                    {
                        tb = new TextButton(new Vector2(UIScaler.GetWidthUnits() - 9, offset), new Vector2(6, 6), "", delegate { Select(id); });
                    }
                    tb.background.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
                    tb.background.transform.parent = scrollArea.transform;
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = bgColor;
                    buttons[id].Add(tb);

                    if (left)
                    {
                        tb = new TextButton(new Vector2(8, offset + 1.5f), new Vector2(UIScaler.GetWidthUnits() - 19, 3), "  " + game.cd.GetContentName(id), delegate { Select(id); }, Color.clear);
                    }
                    else
                    {
                        tb = new TextButton(new Vector2(10, offset + 1.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 3), "  " + game.cd.GetContentName(id), delegate { Select(id); }, Color.clear);
                    }
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = bgColor;
                    tb.background.transform.parent = scrollArea.transform;
                    buttons[id].Add(tb);

                    if (left)
                    {
                        tb = new TextButton(new Vector2(9, offset + 1.5f), new Vector2(UIScaler.GetWidthUnits() - 19, 3), game.cd.GetContentName(id), delegate { Select(id); }, Color.black);
                    }
                    else
                    {
                        tb = new TextButton(new Vector2(11, offset + 1.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 3), game.cd.GetContentName(id), delegate { Select(id); }, Color.black);
                    }
                    tb.setColor(Color.clear);
                    tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
                    tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                    tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                    //tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
                    tb.SetFont(game.gameType.GetHeaderFont());
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = bgColor;
                    tb.background.transform.parent = scrollArea.transform;
                    buttons[id].Add(tb);

                    left = !left;
                    offset += 4f;
                }
            }
            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (offset - 4) * UIScaler.GetPixelsPerUnit());

            // Button for back to main menu
            tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Back", delegate { Destroyer.MainMenu(); }, Color.red);
            tb.SetFont(game.gameType.GetHeaderFont());
        }
        
        public void Update()
        {
            foreach (KeyValuePair<string, List<TextButton>> kv in buttons)
            {
                foreach (TextButton tb in kv.Value)
                {
                    if (game.config.data.Get(game.gameType.TypeName() + "Packs") != null
                        && game.config.data.Get(game.gameType.TypeName() + "Packs").ContainsKey(kv.Key))
                    {
                        tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                    }
                    else
                    {
                        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.4f, 0.4f, 0.4f);
                    }
                }
            }
        }

        // set a pack as selected by id
        public void Select(string id)
        {
            if (game.config.data.Get(game.gameType.TypeName() + "Packs") != null
                && game.config.data.Get(game.gameType.TypeName() + "Packs").ContainsKey(id))
            {
                game.config.data.Remove(game.gameType.TypeName() + "Packs", id);
            }
            else
            {
                game.config.data.Add(game.gameType.TypeName() + "Packs", id, "");
            }
            game.config.Save();
            Update();
        }
    }
}