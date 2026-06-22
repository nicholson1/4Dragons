#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
using TMPro.EditorUtilities;
using UnityEditor.Compilation;
using UnityEngine.TextCore;
using System.Reflection;
using UnityEditor.U2D.Sprites;
using System.Linq;

namespace InputIcons
{
    public static class InputIconsSpritePacker
    {

        static int MAX_ATLAS_SIZE = 4096;

        public static void PackIconSets()
        {
            InputIconsLogger.Log("Packing icon sets into sprite assets ...");
            List<InputIconSetBasicSO> iconSOs = InputIconSetConfiguratorSO.GetAllIconSetsOnConfigurator();
            //first we have to pack all the textures into the atlas (scaling them by the SCALE value)
            //then we have to create the correct sprite importer settings and add the spritesheet slices to that
            //then we have to set up the settings in the TMP_SpriteAsset

            Object lastEntry = null;

            if (iconSOs[0] != null)
            {
                lastEntry = PackIconSet(iconSOs[0]); //pack first set once first to avoid an annoying bug that would cause the first one to not be properly packed
            }

            for (int i = 0; i < iconSOs.Count; i++)
            {
                if (iconSOs != null)
                {
                    lastEntry = PackIconSet(iconSOs[i]);
                }
            }

            EditorGUIUtility.PingObject(lastEntry);
            CompilationPipeline.RequestScriptCompilation();
        }


        public static Object PackIconSet(InputIconSetBasicSO iconSet)
        {
            Object lastEntry = null;

            if (iconSet != null)
            {
                if (iconSet.iconSetName == "")
                {
                    InputIconsLogger.LogError("Device display name must not be empty. Aborting packing icon set " + iconSet.name, iconSet);
                    return null;
                }

                float finalScale = 1.2f;
                bool spriteAssetAlreadyExists = false;
                string outputName = InputIconsManagerSO.Instance.TEXTMESHPRO_SPRITEASSET_FOLDERPATH +iconSet.iconSetName + ".asset";
                Object existing = AssetDatabase.LoadAssetAtPath(outputName, typeof(TMP_SpriteAsset));
                if (existing != null)
                {
                    spriteAssetAlreadyExists = true;
                    finalScale = 1;
                }

                lastEntry = PackInputIconSetToSpriteAtlasAndAssets(iconSet);

                if (lastEntry == null)
                    return null;

                string fullPath = Path.GetFullPath(AssetDatabase.GetAssetPath(lastEntry));
                fullPath = "Assets" + fullPath.Substring(Application.dataPath.Length);
                string path = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(lastEntry));

                fullPath = fullPath.Replace(path + ".png", "");
                path += ".asset";
                fullPath += path;



                TMP_SpriteAsset spriteAsset = (TMP_SpriteAsset)AssetDatabase.LoadAssetAtPath(fullPath, typeof(TMP_SpriteAsset));

                if (spriteAsset)
                {
                    spriteAsset.UpdateLookupTables();
                    //adjust placement of glyphs
                    foreach (TMP_SpriteGlyph glyph in spriteAsset.spriteGlyphTable)
                    {
                        if (!spriteAssetAlreadyExists)
                            glyph.scale *= finalScale;

                        GlyphMetrics metrics = glyph.metrics;
                        metrics.horizontalBearingX = 0;
                        metrics.horizontalBearingY = glyph.glyphRect.height / 4 * 3;

                        metrics.horizontalAdvance = glyph.glyphRect.width;
                        metrics.height = glyph.glyphRect.height;
                        metrics.width = glyph.glyphRect.width;

                        glyph.metrics = metrics;
                    }
                    EditorUtility.SetDirty(spriteAsset);
                }

            }
            return lastEntry;
        }

        public static List<SpriteAssetElement> CreateSpriteAssetElements(InputIconSetBasicSO iconSet)
        {
            List<InputSpriteData> spriteDataList = iconSet.GetAllInputSpriteData();
            List<SpriteAssetElement> elements = new List<SpriteAssetElement>();

            for (int i = 0; i < spriteDataList.Count; i++)
            {
                if (spriteDataList[i].sprite == null)
                    continue;

                var element = new SpriteAssetElement();
                elements.Add(element);

                element.path = AssetDatabase.GetAssetPath(spriteDataList[i].sprite.texture);

                element.name = spriteDataList[i].textMeshStyleTag.ToUpper();
                //Debug.Log(element.name +" "+element.path);

                element.sourceTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false, false);
                element.sourceTexture.LoadImage(File.ReadAllBytes(element.path));
                element.sourceTexture.wrapMode = TextureWrapMode.Clamp; //so we don't get pixels from the other edge when scaling
                element.sourceTexture.filterMode = FilterMode.Bilinear;


                element.outputTexture = element.sourceTexture;
            }
            return elements;
        }

        private static void AddSprites(ISpriteEditorDataProvider dataProvider, Rect[] rects, List<SpriteAssetElement> elements, Texture2D atlasTexture)
        {
            float scaleW = (float)atlasTexture.width;
            float scaleH = (float)atlasTexture.height;

            // Add the Sprite Rect to the list of existing Sprite Rects
            var spriteRects = dataProvider.GetSpriteRects().ToList();

            for (int i = 0; i < elements.Count; i++)
            {

                var pixelRect = new Rect(rects[i].x * scaleW, rects[i].y * scaleH, rects[i].width * scaleW, rects[i].height * scaleH);
                // Define the new Sprite Rect
                var newSprite = new SpriteRect()
                {
                    name = elements[i].name,
                    spriteID = GUID.Generate(),
                    rect = pixelRect
                };
                spriteRects.Add(newSprite);

#if UNITY_2021_2_OR_NEWER
                // Note: This section is only for Unity 2021.2 and newer
                // Register the new Sprite Rect's name and GUID with the ISpriteNameFileIdDataProvider
                var spriteNameFileIdDataProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
                var nameFileIdPairs = spriteNameFileIdDataProvider.GetNameFileIdPairs().ToList();
                nameFileIdPairs.Add(new SpriteNameFileIdPair(newSprite.name, newSprite.spriteID));
                spriteNameFileIdDataProvider.SetNameFileIdPairs(nameFileIdPairs);
                // End of Unity 2021.2 and newer section
#endif
            }

            // Write the updated data back to the data provider
            dataProvider.SetSpriteRects(spriteRects.ToArray());

            // Apply the changes
            dataProvider.Apply();
        }


        public static Object PackInputIconSetToSpriteAtlasAndAssets(InputIconSetBasicSO iconSet)
        {
            InputIconsLogger.Log("pack iconset: " + iconSet.iconSetName + "...");

            List<SpriteAssetElement> elements = CreateSpriteAssetElements(iconSet);
            var textures = elements.ConvertAll<Texture2D>(e => e.outputTexture).ToArray();

            var atlasTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false, false);
            atlasTexture.filterMode = FilterMode.Bilinear;

            Rect[] rects =  atlasTexture.PackTextures(textures, 0, MAX_ATLAS_SIZE, false);

            atlasTexture.Apply(false, false);

            string outputName = InputIconsManagerSO.Instance.TEXTMESHPRO_SPRITEASSET_FOLDERPATH +iconSet.iconSetName;
            string atlasFileName = outputName+".png";

            Texture2D existingAtlas = AssetDatabase.LoadAssetAtPath(atlasFileName, typeof(Texture2D)) as Texture2D;
            if (existingAtlas)
            {
                DeleteSubspritesOfTexture(existingAtlas);
            }

            File.WriteAllBytes(atlasFileName, atlasTexture.EncodeToPNG());

            EditorUtility.SetDirty(atlasTexture);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();



            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(atlasFileName);

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.mipmapEnabled = true;
            importer.mipmapFilter = TextureImporterMipFilter.KaiserFilter;
            importer.textureCompression = TextureImporterCompression.Compressed;
            if (InputIconsManagerSO.Instance.useCompressionForCreatedAssets)
            {
                importer.crunchedCompression = true;
            }

            
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 4096;


            AssetDatabase.ImportAsset(atlasFileName, ImportAssetOptions.ForceUpdate);
            Object finalTexture = AssetDatabase.LoadAssetAtPath(atlasFileName, typeof(Texture2D));
            Selection.activeObject = finalTexture;


            //create sprites and add metadata within the sprite atlas
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(finalTexture);
            dataProvider.InitSpriteEditorDataProvider();

            AddSprites(dataProvider, rects, elements, atlasTexture);

            // Apply the changes made to the data provider
            dataProvider.Apply();

            // Reimport the asset to have the changes applied
            var assetImporter = dataProvider.targetObject as AssetImporter;
            assetImporter.SaveAndReimport();


            //cleanup textures
            foreach (var element in elements)
            {
                Texture2D.DestroyImmediate(element.sourceTexture);
                Texture2D.DestroyImmediate(element.outputTexture);
            }



            outputName += ".asset";
            Object existing = AssetDatabase.LoadAssetAtPath(outputName, typeof(TMP_SpriteAsset));

            if (existing == null)
            {
                //TMP_SpriteAssetMenu.CreateSpriteAsset();

                //the TMP_SpriteAssetMenu.CreateSpriteAsset(); method got changed to private at some point and can not be accessed in later Unity versions
                //therefore we use reflection instead to guarantee we can use the method even if it is private
                System.Type assetMenu = typeof(TMP_SpriteAssetMenu);
                MethodInfo method = assetMenu.GetMethod("CreateSpriteAsset", BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                    method = assetMenu.GetMethod("CreateSpriteAsset", BindingFlags.NonPublic | BindingFlags.Static);
                method.Invoke(null, null);
            }


            if (existing)
            {
                //if there was already an existing SpriteAsset, then call the update method to refresh the asset
                //using reflection again
                TMP_SpriteAsset spriteAsset = existing as TMP_SpriteAsset;

                if (spriteAsset != null)
                {
                    System.Type assetMenu = typeof(TMP_SpriteAssetMenu);
                    System.Type[] parameterTypes = new System.Type[] { typeof(TMP_SpriteAsset) };

                    MethodInfo method = assetMenu.GetMethod("UpdateSpriteAsset", BindingFlags.Public | BindingFlags.Static, null, parameterTypes, null);

                    if (method == null)
                        method = assetMenu.GetMethod("UpdateSpriteAsset", BindingFlags.NonPublic | BindingFlags.Static, null, parameterTypes, null);

                    if (method != null)
                        method.Invoke(null, new object[] { spriteAsset });
                }
            }


            return finalTexture;
        }

        public static void DeleteSubspritesOfTexture(Texture2D texture)
        {
            Object[] existingSpriteData = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture));
            if (existingSpriteData != null)
            {
                foreach (Object obj in existingSpriteData)
                {
                    if (obj == null)
                        continue;

                    if (obj.GetType() == typeof(Sprite))
                    {
                        string objPath = AssetDatabase.GetAssetPath(obj);
                        AssetDatabase.DeleteAsset(objPath);
                    }
                }
            }
        }

        /// <summary>
        /// Extracts sprites from a TMP_SpriteAsset and assigns them back to the corresponding IconSet
        /// </summary>
        /// <param name="iconSet">The icon set to update with sprites from the atlas</param>
        /// <param name="createBackup">If true, creates a backup of the icon set before modifying it</param>
        /// <returns>The backup icon set if created, otherwise null</returns>
        public static InputIconSetBasicSO AssignSpriteAtlasSpritesToIconSet(InputIconSetBasicSO iconSet, bool createBackup = true)
        {
            if (iconSet == null || string.IsNullOrEmpty(iconSet.iconSetName))
            {
                InputIconsLogger.LogError("Invalid icon set. Cannot assign sprites from atlas.", iconSet);
                return null;
            }

            // Load the sprite asset
            string spriteassetPath = InputIconsManagerSO.Instance.TEXTMESHPRO_SPRITEASSET_FOLDERPATH + iconSet.iconSetName + ".asset";
            TMP_SpriteAsset spriteAsset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(spriteassetPath);

            if (spriteAsset == null)
            {
                InputIconsLogger.LogError($"Sprite asset not found at path: {spriteassetPath}. Make sure you've created the sprite atlas first.", iconSet);
                return null;
            }

            InputIconSetBasicSO backupIconSet = null;

            // Create backup if requested
            if (createBackup)
            {
                string iconSetPath = AssetDatabase.GetAssetPath(iconSet);
                string backupPath = iconSetPath.Replace(".asset", "_Backup.asset");
                backupPath = AssetDatabase.GenerateUniqueAssetPath(backupPath);

                // Check if a backup already exists for this asset
                bool backupCreated = AssetDatabase.CopyAsset(iconSetPath, backupPath);

                if (backupCreated)
                {
                    backupIconSet = AssetDatabase.LoadAssetAtPath<InputIconSetBasicSO>(backupPath);

                    if (backupIconSet != null)
                    {
                        InputIconsLogger.Log($"Created backup of icon set at: {backupPath}");
                    }
                    else
                    {
                        InputIconsLogger.LogWarning($"Failed to create backup at: {backupPath}. Continuing without backup.");
                    }
                }
                else
                {
                    InputIconsLogger.LogWarning($"Could not create backup at: {backupPath}. Asset copy operation failed.");
                }
            }

            // Get all sprite data from the icon set
            List<InputSpriteData> spriteDataList = iconSet.GetAllInputSpriteData();
            Dictionary<string, Sprite> atlasSprites = new Dictionary<string, Sprite>();

            // Get all sprites from the sprite asset's texture
            Object[] spriteObjects = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(spriteAsset.spriteSheet));
            foreach (var obj in spriteObjects)
            {
                if (obj is Sprite sprite)
                {
                    atlasSprites[sprite.name.ToUpper()] = sprite;
                }
            }

            int replacedCount = 0;

            // Update sprites in the icon set
            foreach (var spriteData in spriteDataList)
            {
                if (spriteData.sprite != null && !string.IsNullOrEmpty(spriteData.textMeshStyleTag))
                {
                    string lookupKey = spriteData.textMeshStyleTag.ToUpper();

                    if (atlasSprites.TryGetValue(lookupKey, out Sprite atlasSprite))
                    {
                        spriteData.sprite = atlasSprite;
                        replacedCount++;
                    }
                }
            }

            // Handle custom context icons separately
            if (iconSet.customContextIcons != null && iconSet.customContextIcons.Count > 0)
            {
                foreach (var customIcon in iconSet.customContextIcons)
                {
                    if (customIcon.customInputContextSprite != null && !string.IsNullOrEmpty(customIcon.textMeshStyleTag))
                    {
                        string lookupKey = customIcon.textMeshStyleTag.ToUpper();

                        if (atlasSprites.TryGetValue(lookupKey, out Sprite atlasSprite))
                        {
                            customIcon.customInputContextSprite = atlasSprite;
                            replacedCount++;
                        }
                    }
                }
            }

            if (replacedCount > 0)
            {
                EditorUtility.SetDirty(iconSet);
                AssetDatabase.SaveAssets();
                InputIconsLogger.Log($"Updated {replacedCount} sprites in icon set '{iconSet.name}' to use sprites from atlas.");
            }
            else
            {
                InputIconsLogger.LogWarning($"No sprites were replaced in icon set '{iconSet.name}'. Make sure the sprite names match the textMeshStyleTag values.");
            }

            return backupIconSet;
        }

        /// <summary>
        /// Assigns sprite atlas sprites to all icon sets in the configurator
        /// </summary>
        /// <param name="createBackups">If true, creates backups of all icon sets before modifying them</param>
        /// <returns>List of backup icon sets if created</returns>
        public static List<InputIconSetBasicSO> AssignSpriteAtlasSpritesToAllIconSets(bool createBackups = true)
        {
            List<InputIconSetBasicSO> backups = new List<InputIconSetBasicSO>();
            List<InputIconSetBasicSO> iconSets = InputIconSetConfiguratorSO.GetAllIconSetsOnConfigurator();

            // Track which icon sets have already been backed up (by asset path)
            HashSet<string> backedUpPaths = new HashSet<string>();

            foreach (var iconSet in iconSets)
            {
                if (iconSet != null)
                {
                    // Check if this exact asset has already been backed up
                    string assetPath = AssetDatabase.GetAssetPath(iconSet);
                    bool needsBackup = createBackups && !backedUpPaths.Contains(assetPath);

                    var backup = AssignSpriteAtlasSpritesToIconSet(iconSet, needsBackup);
                    if (backup != null)
                    {
                        backups.Add(backup);
                        backedUpPaths.Add(assetPath);
                    }
                    else if (needsBackup)
                    {
                        // Even if backup creation failed, mark as processed
                        backedUpPaths.Add(assetPath);
                    }
                }
            }

            return backups;
        }


        public class SpriteAssetElement
        {
            public string path;
            public string name;

            public Texture2D sourceTexture;
            public Texture2D outputTexture;

            public Rect rect;
            public SpriteMetaData meta;
            public TMP_Sprite tmpSprite;
        }
    }
}
#endif