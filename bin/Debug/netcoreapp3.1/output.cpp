struct ItemDisplayInfoLoadInfo
{
     static DB2LoadInfo const* Instance()
      {
         static DB2FieldMeta const fields[] =
          {
             {true, FT_INT, ID},
             {true, FT_INT, ItemVisual},
             {true, FT_INT, ParticleColorID},
             {false, FT_INT, ItemRangedDisplayInfoID},
             {false, FT_INT, OverrideSwooshSoundKitID},
             {true, FT_INT, SheatheTransformMatrixID},
             {true, FT_INT, StateSpellVisualKitID},
             {true, FT_INT, SheathedSpellVisualKitID},
             {false, FT_INT, UnsheathedSpellVisualKitID},
             {true, FT_INT, Flags},
             {false, FT_INT, ModelResourcesID1},
             {false, FT_INT, ModelResourcesID2},
             {true, FT_INT, ModelMaterialResourcesID1},
             {true, FT_INT, ModelMaterialResourcesID2},
             {true, FT_INT, Field_8_2_0_30080_011_1},
             {true, FT_INT, Field_8_2_0_30080_011_2},
             {true, FT_INT, GeosetGroup1},
             {true, FT_INT, GeosetGroup2},
             {true, FT_INT, GeosetGroup3},
             {true, FT_INT, GeosetGroup4},
             {true, FT_INT, GeosetGroup5},
             {true, FT_INT, GeosetGroup6},
             {true, FT_INT, AttachmentGeosetGroup1},
             {true, FT_INT, AttachmentGeosetGroup2},
             {true, FT_INT, AttachmentGeosetGroup3},
             {true, FT_INT, AttachmentGeosetGroup4},
             {true, FT_INT, AttachmentGeosetGroup5},
             {true, FT_INT, AttachmentGeosetGroup6},
             {true, FT_INT, HelmetGeosetVis1},
             {true, FT_INT, HelmetGeosetVis2},
          };
 static DB2LoadInfo const loadInfo($fields[0], std::extent<decltype(fields)>::value,ItemDisplayInfo::Instance(), HOTFIX_SEL_ITEM_DISPLAY_INFO)
 return $loadInfo;
