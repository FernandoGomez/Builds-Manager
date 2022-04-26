﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;
using Microsoft.Xna.Framework.Graphics;
using Gw2Sharp.ChatLinks;
using Microsoft.Xna.Framework.Input;
using Blish_HUD.Input;
using System.Text.RegularExpressions;
using MonoGame.Extended.BitmapFonts;

namespace Kenedia.Modules.BuildsManager
{
    public class CustomTooltip : Control
    {
        private Texture2D Background;
        private Texture2D Icon;
        public string Header;
        public List<string> Content;
        public CustomTooltip(Container parent)
        {
            Parent = GameService.Graphics.SpriteScreen;

            Size = new Point(225, 275);
            Background = BuildsManager.TextureManager._Backgrounds[(int)_Backgrounds.Tooltip];
            ZIndex = 1000;
            Visible = false;


            Input.Mouse.MouseMoved += delegate
            {
                Location = Input.Mouse.Position.Add(new Point(20, -10));
            };
        }
        void UpdateLayout()
        {
            if (Header == null || Content == null) return;

            var cnt = new ContentService();
            var font = cnt.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)14, ContentService.FontStyle.Regular);
            var headerFont = cnt.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)18, ContentService.FontStyle.Regular);

            var ItemNameSize = headerFont.GetStringRectangle(Header);

            var width = (int)ItemNameSize.Width + 30;
            var height = 10 + (int)ItemNameSize.Height;
            List<string> newStrings = new List<string>();
            foreach (string s in Content)
            {
                var ss = Regex.Replace(s, "<c=@reminder>", Environment.NewLine + Environment.NewLine);
                ss = Regex.Replace(ss, "</c>", "");
                ss = Regex.Replace(ss, "<br>", "");
                newStrings.Add(ss);

                var rect = font.GetStringRectangle(ss);
                width = Math.Max(width, Math.Min((int)rect.Width + 30, 300));

                height += (int)(rect.Height);
                height += (int)((int)(rect.Width / (width - 20)) * (font.LineHeight));
            }
            Content = newStrings;

            var firstWidth = font.MeasureString(Content[0]).Width;

            Height = height + (Content.Count == 1 ? firstWidth > (width - 20) ? font.LineHeight : 20 : Content.Count == 6 ? 20 : 20);
            Width = width;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Header == null || Content == null) return;
            UpdateLayout();

            var cnt = new ContentService();
            var font = cnt.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)14, ContentService.FontStyle.Regular);
            var headerFont = cnt.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)18, ContentService.FontStyle.Regular);

            var rect = font.GetStringRectangle(Header);

            spriteBatch.DrawOnCtrl(this,
                                    ContentService.Textures.Pixel,
                                    bounds,
                                    bounds,
                                    new Color(55, 55, 55, 255),
                                    0f,
                                    default);

            spriteBatch.DrawOnCtrl(this,
                                    Background,
                                    bounds.Add(2, 0, 0, 0),
                                    bounds,
                                    Color.White,
                                    0f,
                                    default);

            var color = Color.Black;
            //Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            //Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);


            spriteBatch.DrawStringOnCtrl(this,
                                   Header,
                                   headerFont,
                                   new Rectangle(10, 10, 0, (int)rect.Height),
                                   Color.Orange,
                                   false,
                                   HorizontalAlignment.Left
                                   );

            spriteBatch.DrawStringOnCtrl(this,
                                   string.Join(Environment.NewLine, Content),
                                   font,
                                   new Rectangle(10, (int)rect.Height + 25, Width - 10, Height),
                                   Color.Honeydew,
                                   true,
                                   HorizontalAlignment.Left,
                                   VerticalAlignment.Top
                                   );
        }
    }

    public class SelectionPopUp : Control
    {
        public class SelectionEntry
        {
            public object Object;
            public Texture2D Texture;
            public string Header;
            public List<string> Content;
            public List<Texture2D> ContentTextures;
            public int UpgradeIndex = 0;

            public Rectangle TextureBounds;
            public Rectangle TextBounds;
            public List<Rectangle> ContentBounds;
            public Rectangle Bounds;
            public bool Hovered;
        }
        public enum selectionType
        {
            Runes,
            Sigils,
            Stats,
            Weapons,
            AquaticSigils,
        }

        private Texture2D Background;
        private TextBox FilterBox;
        public selectionType SelectionType;
        public List<SelectionEntry> List = new List<SelectionEntry>();
        public List<SelectionEntry> FilteredList = new List<SelectionEntry>();
        public object SelectionTarget;

        private ContentService ContentService;
        private BitmapFont Font;
        private BitmapFont HeaderFont;
        private Scrollbar Scrollbar;

        public SelectionPopUp(Container parent)
        {            
            Parent = parent;
            Visible = false;
            ZIndex = 997;
            Size = new Point(300, 500);
            Background = BuildsManager.TextureManager._Backgrounds[(int)_Backgrounds.Tooltip];
            //BackgroundColor = Color.Honeydew;
            FilterBox = new TextBox()
            {
                Parent = Parent,
                PlaceholderText = "Search ...",
                Width = Width - 6,
                ZIndex = 998,
                Visible = false,
            };

            ContentService = new ContentService();
            Font = ContentService.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)14, ContentService.FontStyle.Regular);
            HeaderFont = ContentService.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)18, ContentService.FontStyle.Regular);

            Click += OnClick;

            Moved += delegate
            {
                FilterBox.Location = Location.Add(new Point(3, 4));
            };

            Resized += delegate
            {
                FilterBox.Width = Width - 6;
            };

            Hidden += delegate
            {
                FilterBox.Hide();
            };

            Shown += delegate
            {
                FilterBox.Show();
            };
            Disposed += delegate
            {
                FilterBox.Dispose();
            };
        }

        private void PrepareData()
        {
            switch (SelectionType)
            {
                case selectionType.Runes:
                    break;

                case selectionType.Sigils:
                    break;

                case selectionType.Stats:
                    break;

                case selectionType.Weapons:
                    break;
            }
        }

        private void OnClick(object sender, MouseEventArgs mouse)
        {
            if (List == null || List.Count == 0) return;

            foreach (SelectionEntry entry in List)
            {
                if (entry.Hovered)
                {
                    switch (SelectionType)
                    {
                        case selectionType.Runes:
                            var rune = (API.RuneItem)entry.Object;
                            var armor = (Armor_TemplateItem)SelectionTarget;
                            armor.Rune = rune;
                            break;

                        case selectionType.Sigils:
                            var sigil = (API.SigilItem)entry.Object;
                            var weapon = (Weapon_TemplateItem)SelectionTarget;
                            weapon.Sigil = sigil;
                            break;

                        case selectionType.AquaticSigils:
                            var aquaSigil = (API.SigilItem)entry.Object;
                            var aquaWeapon = (AquaticWeapon_TemplateItem)SelectionTarget;
                            aquaWeapon.Sigils[entry.UpgradeIndex] = aquaSigil;
                            break;

                        case selectionType.Stats:
                            var stat = (API.Stat)entry.Object;
                            var item = (TemplateItem)SelectionTarget;
                            item.Stat = stat;
                            break;

                        case selectionType.Weapons:
                            var selectedWeapon = (API.weaponType)entry.Object;
                            var iWeapon = (Weapon_TemplateItem)SelectionTarget;
                            iWeapon.WeaponType = selectedWeapon;
                            break;
                    }

                    Hide();
                }
            }
        }
        class filterTag
        {
            public string text;
            public bool match;
        }
        private void UpdateLayout()
        {
            if (List == null || List.Count == 0) return;

            int i = 0;
            int size = 42;

            FilteredList = new List<SelectionEntry>();
            if (FilterBox.Text != null && FilterBox.Text != "")
            {
                List<string> tags = FilterBox.Text.ToLower().Split(' ').ToList();
                var filteredTags = tags.Where(e => e.Trim().Length > 0);

                foreach (SelectionEntry entry in List)
                {
                    List<filterTag> Tags = new List<filterTag>();

                    foreach (string t in filteredTags)
                    {
                        var tag = new filterTag()
                        {
                            text = t.Trim().ToLower(),
                            match = false,
                        };
                        Tags.Add(tag);

                        if (entry.Header.ToLower().Contains(tag.text))
                        {
                            FilteredList.Add(entry);
                            tag.match = true;
                        }

                        foreach (string s in entry.Content)
                        {
                            var lower = s.ToLower();

                            tag.match = tag.match ? tag.match : lower.Contains(tag.text);
                            if (tag.match) break;
                        }
                    }

                    if(!FilteredList.Contains(entry) && (Tags.Count == Tags.Where(e => e.match == true).ToList().Count)) FilteredList.Add(entry);
                }
            }
            else
            {
                FilteredList = new List<SelectionEntry>(List);
            }

            foreach (SelectionEntry entry in FilteredList)
            {
                entry.Hovered = new Rectangle(0, FilterBox.Height + 5 + i * (size + 5), Width, size).Contains(RelativeMousePosition);
                entry.TextureBounds = new Rectangle(0, FilterBox.Height + 5 + i * (size + 5), size, size);
                entry.TextBounds = new Rectangle(size + 5, FilterBox.Height + i * (size + 5), size, size);
                entry.ContentBounds = new List<Rectangle>();

                int j = 0;
                int statSize = Font.LineHeight;
                foreach (Texture2D texture in entry.ContentTextures)
                {
                    entry.ContentBounds.Add(new Rectangle(size + j * statSize, FilterBox.Height + Font.LineHeight + 12 + i * (size + 5), statSize, statSize));
                    j++;
                }
                i++;
            }

            Height = FilterBox.Height + 5 + Math.Min(10, Math.Max(FilteredList.Count, 1))* (size + 5);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // if (List == null || List.Count == 0) return;

            PrepareData();
            UpdateLayout();

            spriteBatch.DrawOnCtrl(this,
                                    ContentService.Textures.Pixel,
                                    bounds,
                                    bounds,
                                    new Color(75, 75, 75, 255),
                                    0f,
                                    default);

            spriteBatch.DrawOnCtrl(this,
                                    Background,
                                    bounds.Add(-2,0,0,0),
                                    bounds,
                                    Color.White,
                                    0f,
                                    default);

            var color = Color.Black;
            //Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom -2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom -1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);
            
            //Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            int i = 0;
            int size = 42;
            foreach (SelectionEntry entry in FilteredList)
            {
                spriteBatch.DrawOnCtrl(this,
                                        entry.Texture,
                                        entry.TextureBounds,
                                        entry.Texture.Bounds,
                                       entry.Hovered ? Color.Orange : Color.White,
                                        0f,
                                        default);

                spriteBatch.DrawStringOnCtrl(this,
                                        entry.Header,
                                        Font,
                                        entry.TextBounds,
                                       entry.Hovered ? Color.Orange : Color.White,
                                        false,
                                        HorizontalAlignment.Left);

                if (entry.ContentTextures.Count > 0)
                {
                    int j = 0;
                    int statSize = Font.LineHeight;
                    foreach (Texture2D texture in entry.ContentTextures)
                    {
                        spriteBatch.DrawOnCtrl(this,
                                                texture,
                                                entry.ContentBounds[j],
                                                texture.Bounds,
                                       entry.Hovered ? Color.Orange : Color.White,
                                                0f,
                                                default);
                        j++;
                    }
                }
                else
                {
                    spriteBatch.DrawStringOnCtrl(this,
                                            string.Join("; ", entry.Content),
                                            Font,
                                            new Rectangle(size + 5, Font.LineHeight + FilterBox.Height + i * (size + 5), size, size),
                                            Color.LightGray,
                                            false,
                                            HorizontalAlignment.Left);
                }
                i++;
            }
        }
    }

    public class Control_Equipment : Control
    {
        private Template _Template;
        public Template Template
        {
            get => _Template;
            set
            {
                if (value != null)
                {
                    _Template = value;
                    UpdateTemplate();
                }
            }
        }

        public double Scale;
        private Texture2D _RuneTexture;
        private List<API.TrinketItem> Trinkets = new List<API.TrinketItem>();
        private List<API.ArmorItem> Armors = new List<API.ArmorItem>();
        private List<API.WeaponItem> Weapons = new List<API.WeaponItem>();
        private List<Texture2D> WeaponSlots = new List<Texture2D>();
        private List<Texture2D> AquaticWeaponSlots = new List<Texture2D>();

        private List<SelectionPopUp.SelectionEntry> Stats_Selection = new List<SelectionPopUp.SelectionEntry>();
        private List<SelectionPopUp.SelectionEntry> Sigils_Selection = new List<SelectionPopUp.SelectionEntry>();
        private List<SelectionPopUp.SelectionEntry> Runes_Selection = new List<SelectionPopUp.SelectionEntry>();
        private List<SelectionPopUp.SelectionEntry> Weapons_Selection = new List<SelectionPopUp.SelectionEntry>();

        private string _Profession;
        public CustomTooltip CustomTooltip;
        public SelectionPopUp SelectionPopUp;

        public Control_Equipment(Container parent)
        {
            Parent = parent;
            // BackgroundColor = Color.Aqua;
            _RuneTexture = BuildsManager.TextureManager.getEquipTexture(_EquipmentTextures.Rune).GetRegion(37, 37, 54, 54);


            List<int> intList = new List<int>() { 1, 2, 3 };
            List<string> stringList = new List<string>() { "A", "B", "C" };
            List<Texture2D> textureList = new List<Texture2D>() { _RuneTexture, _RuneTexture, _RuneTexture };
            var list = textureList;
            for (int i = 0; i < list.Count; i++)
            {
                var entry = list[i];
                BuildsManager.Logger.Debug("It's a {0}", entry.GetType().Name);

                switch (entry.GetType().Name)
                {
                    case "String":
                        BuildsManager.Logger.Debug("It's a {0}", "string");
                        break;

                    case "Int32":
                        BuildsManager.Logger.Debug("It's a {0}", "int");
                        break;

                    case "Texture2D":
                        BuildsManager.Logger.Debug("It's a {0}", "Texture2D");
                        break;
                }
            }

            SelectionPopUp = new SelectionPopUp(GameService.Graphics.SpriteScreen)
            {

            };


            Trinkets = new List<API.TrinketItem>();
            foreach (API.TrinketItem item in BuildsManager.Data.Trinkets)
            {
                Trinkets.Add(item);
            }

            WeaponSlots = new List<Texture2D>()
            {
                BuildsManager.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.Weapon1_MainHand],
                BuildsManager.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.Weapon1_OffHand],

                BuildsManager.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.Weapon2_MainHand],
                BuildsManager.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.Weapon2_OffHand],
            };

            AquaticWeaponSlots = new List<Texture2D>()
            {
                BuildsManager.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.AquaticWeapon1],
                BuildsManager.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.AquaticWeapon2],
            };

            Weapons = new List<API.WeaponItem>() { };
            foreach (API.WeaponItem weapon in BuildsManager.Data.Weapons) { Weapons.Add(weapon); }

            Click += OnClick;
            Input.Mouse.LeftMouseButtonPressed += OnGlobalClick;

            CustomTooltip = new CustomTooltip(Parent)
            {
                ClipsBounds = false,
            };
            Disposed += delegate
            {
                CustomTooltip.Dispose();
                SelectionPopUp.Dispose();
            };

            foreach(API.RuneItem item in BuildsManager.Data.Runes)
            {
                Runes_Selection.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = item,
                    Texture = item.Icon.Texture,
                    Header = item.Name,
                    Content = item.Bonuses,
                });
            }

            foreach(API.SigilItem item in BuildsManager.Data.Sigils)
            {
                Sigils_Selection.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = item,
                    Texture = item.Icon.Texture,
                    Header = item.Name,
                    Content = new List<string>() { item.Description },
                });
            }

            foreach(API.WeaponItem item in Weapons)
            {
                Weapons_Selection.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = item,
                    Texture = item.Icon.Texture,
                    Header = item.WeaponType.ToString(),
                    Content = new List<string>() { item.Slot.ToString() },
                });
            }

            foreach(API.Stat item in BuildsManager.Data.Stats)
            {
                Stats_Selection.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = item,
                    Texture = item.Icon.Texture,
                    Header = item.Name,
                    Content = item.Attributes.Select(e => "+ " + e.Name).ToList(),
                    ContentTextures = item.Attributes.Select(e => e.Icon.Texture).ToList(),
                });
            }
        }

        public EventHandler Changed;
        private void OnChanged()
        {
            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        private void OnGlobalClick(object sender, MouseEventArgs m)
        {
            if (!MouseOver) SelectionPopUp.Hide();
        }

        private void OnClick(object sender, MouseEventArgs m)
        {
            SelectionPopUp.Hide();

            foreach (TemplateItem item in Template.Gear.Trinkets)
            {
                if (item.Hovered)
                {
                    SelectionPopUp.Show();
                    SelectionPopUp.Location = new Point(Input.Mouse.Position.X - RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - RelativeMousePosition.Y + item.Bounds.Y - 1);
                    SelectionPopUp.SelectionType = SelectionPopUp.selectionType.Stats;
                    SelectionPopUp.SelectionTarget = item;
                    SelectionPopUp.List = Stats_Selection;
                }
            }

            foreach (Armor_TemplateItem item in Template.Gear.Armor)
            {
                if (item.Hovered)
                {
                    SelectionPopUp.Show();
                    SelectionPopUp.Location = new Point(Input.Mouse.Position.X - RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - RelativeMousePosition.Y + item.Bounds.Y - 1);
                }
            }

            foreach (Weapon_TemplateItem item in Template.Gear.Weapons)
            {
                if (item.Hovered)
                {
                    SelectionPopUp.Show();
                    SelectionPopUp.Location = new Point(Input.Mouse.Position.X - RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - RelativeMousePosition.Y + item.Bounds.Y - 1);
                }
            }
            foreach (AquaticWeapon_TemplateItem item in Template.Gear.AquaticWeapons)
            {
                if (item.Hovered)
                {
                    SelectionPopUp.Show();
                    SelectionPopUp.Location = new Point(Input.Mouse.Position.X - RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - RelativeMousePosition.Y + item.Bounds.Y - 1);
                }
            }

        }

        private void UpdateTemplate()
        {

        }

        private void ProfessionChanged()
        {
            _Profession = Template.Build.Profession.Id;
            var profession = Template.Build.Profession;
            var armorWeight = API.armorWeight.Heavy;

            switch (profession.Id)
            {
                case "Elementalist":
                case "Necromancer":
                case "Mesmer":
                    armorWeight = API.armorWeight.Light;
                    break;

                case "Ranger":
                case "Thief":
                case "Engineer":
                    armorWeight = API.armorWeight.Medium;
                    break;

                case "Warrior":
                case "Guardian":
                case "Revenant":
                    armorWeight = API.armorWeight.Heavy;
                    break;
            }

            Armors = new List<API.ArmorItem>()
            {
                new API.ArmorItem(),
                new API.ArmorItem(),
                new API.ArmorItem(),
                new API.ArmorItem(),
                new API.ArmorItem(),
                new API.ArmorItem(),
            };
            foreach (API.ArmorItem armor in BuildsManager.Data.Armors)
            {
                if (armor.ArmorWeight == armorWeight)
                {
                    Armors[(int)armor.Slot] = armor;
                }
            }
        }

        private void UpdateLayout()
        {
            Point mPos = RelativeMousePosition;
            int i;
            int offset = 1;
            int size = 48;
            int statSize = (int)(size / 1.5);

            if (CustomTooltip.Visible) CustomTooltip.Visible = false;

            i = 0;
            foreach (TemplateItem item in Template.Gear.Trinkets)
            {
                item.Bounds = new Rectangle(offset, 5 + i * (size + 6), size, size);
                item.UpgradeBounds = new Rectangle(offset + size + 8, 5 + i * (size + 6), size, size);
                item.StatBounds = new Rectangle(offset + (size - statSize), 5 + i * (size + 6) + (size - statSize), statSize, statSize);
                item.Hovered = item.Bounds.Contains(mPos);
                if (item.Hovered && item.Stat != null && !SelectionPopUp.MouseOver)
                {
                    CustomTooltip.Visible = true;
                    CustomTooltip.Header = item.Stat.Name;
                    CustomTooltip.Content = new List<string>();
                    foreach (API.StatAttribute attribute in item.Stat.Attributes)
                    {
                        CustomTooltip.Content.Add("+ " + Math.Round(attribute.Multiplier * Trinkets[i].AttributeAdjustment) + " " + attribute.Name);
                    }
                };

                i++;
            }

            i = 0;
            offset += 90;
            foreach (Armor_TemplateItem item in Template.Gear.Armor)
            {
                item.Bounds = new Rectangle(offset, 5 + i * (size + 6), size, size);
                item.UpgradeBounds = new Rectangle(offset + size + 8, 5 + i * (size + 6), size, size);
                item.StatBounds = new Rectangle(offset + (size - statSize), 5 + i * (size + 6) + (size - statSize), statSize, statSize);
                item.Hovered = item.Bounds.Contains(mPos);

                if (!SelectionPopUp.MouseOver)
                {
                    if (item.UpgradeBounds.Contains(mPos))
                    {
                        CustomTooltip.Visible = true;
                        CustomTooltip.Header = item.Rune.Name;
                        CustomTooltip.Content = item.Rune.Bonuses;
                    }
                    else if (item.Hovered && item.Stat != null)
                    {
                        CustomTooltip.Visible = true;
                        CustomTooltip.Header = item.Stat.Name;
                        CustomTooltip.Content = new List<string>();
                        foreach (API.StatAttribute attribute in item.Stat.Attributes)
                        {
                            CustomTooltip.Content.Add("+ " + Math.Round(attribute.Multiplier * Trinkets[i].AttributeAdjustment) + " " + attribute.Name);
                        }
                    }
                }

                i++;
            }

            i = 0;
            offset += 150;
            foreach (Weapon_TemplateItem item in Template.Gear.Weapons)
            {
                item.Bounds = new Rectangle(offset, 5 + i * (size + 6), size, size);
                item.UpgradeBounds = new Rectangle(offset + size + 8, 5 + i * (size + 6), size, size);

                item.StatBounds = new Rectangle(offset + (size - statSize), 5 + i * (size + 6) + (size - statSize), statSize, statSize);
                item.Hovered = item.Bounds.Contains(mPos);

                if (!SelectionPopUp.MouseOver)
                {
                    if (item.UpgradeBounds.Contains(mPos))
                    {
                        CustomTooltip.Visible = true;
                        CustomTooltip.Header = item.Sigil.Name;
                        CustomTooltip.Content = new List<string>() { item.Sigil.Description };
                    }
                    else if (item.Hovered && item.Stat != null)
                    {
                        CustomTooltip.Visible = true;
                        CustomTooltip.Header = item.Stat.Name;
                        CustomTooltip.Content = new List<string>();
                        foreach (API.StatAttribute attribute in item.Stat.Attributes)
                        {
                            CustomTooltip.Content.Add("+ " + Math.Round(attribute.Multiplier * Trinkets[i].AttributeAdjustment) + " " + attribute.Name);
                        }
                    }
                }

                if (i == 1) i++;
                i++;
            }

            i = 0;
            offset += 150;
            foreach (AquaticWeapon_TemplateItem item in Template.Gear.AquaticWeapons)
            {
                item.Bounds = new Rectangle(offset, 5 + i * (size + 6), size, size);
                item.UpgradeBounds = new Rectangle(offset + size + 8, 5 + i * (size + 6), size, size);

                for (int j = 0; j < 2; j++)
                {
                    item.SigilsBounds[j] = new Rectangle(item.UpgradeBounds.X, item.UpgradeBounds.Y + 1 + (item.UpgradeBounds.Height / 2 * j), item.UpgradeBounds.Width / 2 - 2, item.UpgradeBounds.Height / 2 - 2);

                    if (!SelectionPopUp.MouseOver)
                    {
                        if (item.SigilsBounds[j].Contains(mPos) && item.Sigils[j] != null)
                        {
                            CustomTooltip.Visible = true;
                            CustomTooltip.Header = item.Sigils[j].Name;
                            CustomTooltip.Content = new List<string>() { item.Sigils[j].Description };
                        }
                    }
                }

                item.StatBounds = new Rectangle(offset + (size - statSize), 5 + i * (size + 6) + (size - statSize), statSize, statSize);
                item.Hovered = item.Bounds.Contains(mPos);

                if (!SelectionPopUp.MouseOver)
                {
                    if (item.Hovered && item.Stat != null)
                    {
                        CustomTooltip.Visible = true;
                        CustomTooltip.Header = item.Stat.Name;
                        CustomTooltip.Content = new List<string>();
                        foreach (API.StatAttribute attribute in item.Stat.Attributes)
                        {
                            CustomTooltip.Content.Add("+ " + Math.Round(attribute.Multiplier * Trinkets[i].AttributeAdjustment) + " " + attribute.Name);
                        }
                    }
                }

                if (i == 0) i = i + 2;
                i++;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_Template == null) return;
            if (_Profession != Template.Build.Profession.Id) ProfessionChanged();

            UpdateLayout();
            int i;
            Color itemColor = new Color(75, 75, 75, 255);
            Color frameColor = new Color(125, 125, 125, 255);

            i = 0;
            foreach (Armor_TemplateItem item in Template.Gear.Armor)
            {

                spriteBatch.DrawOnCtrl(this,
                                        ContentService.Textures.Pixel,
                                        item.Bounds.Add(new Rectangle(-1, -1, 2, 2)),
                                        Rectangle.Empty,
                                        frameColor,
                                        0f,
                                        default);

                spriteBatch.DrawOnCtrl(this,
                                        Armors[i].Icon.Texture,
                                        item.Bounds,
                                        Armors[i].Icon.Texture.Bounds,
                                        itemColor,
                                        0f,
                                        default);

                spriteBatch.DrawOnCtrl(this,
                                        item.Stat.Icon.Texture,
                                        item.StatBounds,
                                        item.Stat.Icon.Texture.Bounds,
                                        Color.White,
                                        0f,
                                        default);


                spriteBatch.DrawOnCtrl(this,
                                        ContentService.Textures.Pixel,
                                        item.UpgradeBounds.Add(new Rectangle(-1, -1, 2, 2)),
                                        Rectangle.Empty,
                                        frameColor,
                                        0f,
                                        default);

                spriteBatch.DrawOnCtrl(this,
                                        item.Rune == null ? _RuneTexture : item.Rune.Icon.Texture,
                                        item.UpgradeBounds,
                                        item.Rune == null ? _RuneTexture.Bounds : item.Rune.Icon.Texture.Bounds,
                                        Color.White,
                                        0f,
                                        default);


                i++;
            }

            i = 0;
            foreach (TemplateItem item in Template.Gear.Trinkets)
            {
                spriteBatch.DrawOnCtrl(this,
                                        ContentService.Textures.Pixel,
                                        item.Bounds.Add(new Rectangle(-1, -1, 2, 2)),
                                        Rectangle.Empty,
                                        frameColor,
                                        0f,
                                        default);

                spriteBatch.DrawOnCtrl(this,
                                        Trinkets[i].Icon.Texture,
                                        item.Bounds,
                                        Trinkets[i].Icon.Texture.Bounds,
                                        itemColor,
                                        0f,
                                        default);

                spriteBatch.DrawOnCtrl(this,
                                        item.Stat.Icon.Texture,
                                        item.StatBounds,
                                        item.Stat.Icon.Texture.Bounds,
                                        Color.White,
                                        0f,
                                        default);


                i++;
            }

            i = 0;
            foreach (Weapon_TemplateItem item in Template.Gear.Weapons)
            {
                spriteBatch.DrawOnCtrl(this,
                                        ContentService.Textures.Pixel,
                                        item.Bounds.Add(new Rectangle(-1, -1, 2, 2)),
                                        Rectangle.Empty,
                                        frameColor,
                                        0f,
                                        default);

                spriteBatch.DrawOnCtrl(this,
                                        item.WeaponType != API.weaponType.Unkown ? Weapons[(int)item.WeaponType].Icon.Texture : WeaponSlots[i],
                                        item.Bounds,
                                        item.WeaponType != API.weaponType.Unkown ? Weapons[(int)item.WeaponType].Icon.Texture.Bounds : WeaponSlots[i].Bounds,
                                        item.WeaponType != API.weaponType.Unkown ? itemColor : Color.White,
                                        0f,
                                        default);

                spriteBatch.DrawOnCtrl(this,
                                        item.Stat.Icon.Texture,
                                        item.StatBounds,
                                        item.Stat.Icon.Texture.Bounds,
                                        Color.White,
                                        0f,
                                        default);

                spriteBatch.DrawOnCtrl(this,
                                        ContentService.Textures.Pixel,
                                        item.UpgradeBounds.Add(new Rectangle(-1, -1, 2, 2)),
                                        Rectangle.Empty,
                                        frameColor,
                                        0f,
                                        default);

                spriteBatch.DrawOnCtrl(this,
                                        item.Sigil == null ? _RuneTexture : item.Sigil.Icon.Texture,
                                        item.UpgradeBounds,
                                        item.Sigil == null ? _RuneTexture.Bounds : item.Sigil.Icon.Texture.Bounds,
                                        Color.White,
                                        0f,
                                        default);


                i++;
            }

            i = 0;
            foreach (AquaticWeapon_TemplateItem item in Template.Gear.AquaticWeapons)
            {
                spriteBatch.DrawOnCtrl(this,
                                        ContentService.Textures.Pixel,
                                        item.Bounds.Add(new Rectangle(-1, -1, 2, 2)),
                                        Rectangle.Empty,
                                        frameColor,
                                        0f,
                                        default);

                spriteBatch.DrawOnCtrl(this,
                                        item.WeaponType != API.weaponType.Unkown ? Weapons[(int)item.WeaponType].Icon.Texture : AquaticWeaponSlots[i],
                                        item.Bounds,
                                        item.WeaponType != API.weaponType.Unkown ? Weapons[(int)item.WeaponType].Icon.Texture.Bounds : AquaticWeaponSlots[i].Bounds,
                                        item.WeaponType != API.weaponType.Unkown ? itemColor : Color.White,
                                        0f,
                                        default);

                spriteBatch.DrawOnCtrl(this,
                                        item.Stat.Icon.Texture,
                                        item.StatBounds,
                                        item.Stat.Icon.Texture.Bounds,
                                        Color.White,
                                        0f,
                                        default);

                for (int j = 0; j < 2; j++)
                {
                    var sigil = item.Sigils.Count > j ? item.Sigils[j] : null;

                    spriteBatch.DrawOnCtrl(this,
                                            ContentService.Textures.Pixel,
                                            item.SigilsBounds[j].Add(new Rectangle(-1, -1, 2, 2)),
                                            Rectangle.Empty,
                                            frameColor,
                                            0f,
                                            default);

                    spriteBatch.DrawOnCtrl(this,
                                            sigil == null ? _RuneTexture : sigil.Icon.Texture,
                                            item.SigilsBounds[j],
                                            sigil == null ? _RuneTexture.Bounds : sigil.Icon.Texture.Bounds,
                                            Color.White,
                                            0f,
                                            default);
                }

                i++;
            }
        }
    }
}
