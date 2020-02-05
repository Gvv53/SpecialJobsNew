
--CREATE PROCEDURE [Merge]
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    MERGE INTO [Order] AS [Target]
--    USING [Order_TEMP] AS [Source]
--     ON Target.Id = Source.Id
--    WHEN MATCHED THEN
--     UPDATE SET 
--     Target.Date = Source.Date, 
--     Target.Number = Source.Number,
--     Target.Text = Source.Text
--    WHEN NOT MATCHED THEN
--    INSERT 
--           (Date, Number, Text) 
--    VALUES 
--           (Source.Date, Source.Number, Source.Text);
--END
USE [Methods]
GO
/****** Object:  StoredProcedure [dbo].[Merge]    Script Date: 24.04.2019 10:17:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [Merge]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    MERGE INTO MEASURING_DATA AS Target
    USING MEASURING_DATA_TEMP AS Source
     ON Target.MDA_ID = Source.MDA_ID
    WHEN MATCHED THEN
     UPDATE SET 
	 Target.MDA_MODE_ID = Source.MDA_MODE_ID,
	 Target.MDA_F = Source.MDA_F, 
	 Target.MDA_F_UNIT_ID = Source.MDA_F_UNIT_ID, 
	 Target.MDA_I = Source.MDA_I, 
	 Target.MDA_ECN_VALUE_IZM = Source.MDA_ECN_VALUE_IZM, 
	 Target.MDA_EN_VALUE_IZM = Source.MDA_EN_VALUE_IZM, 
	 Target.MDA_U0CN_VALUE_IZM = Source.MDA_U0CN_VALUE_IZM, 
	 Target.MDA_U0N_VALUE_IZM = Source.MDA_U0N_VALUE_IZM, 
     Target.MDA_UFCN_VALUE_IZM = Source.MDA_UFCN_VALUE_IZM, 
	 Target.MDA_UFN_VALUE_IZM = Source.MDA_UFN_VALUE_IZM, 
	 Target.MDA_ES_VALUE_IZM = Source.MDA_ES_VALUE_IZM, 
	 Target.MDA_ECN_VALUE_IZM_DB = Source.MDA_ECN_VALUE_IZM_DB, 
	 Target.MDA_EN_VALUE_IZM_DB = Source.MDA_EN_VALUE_IZM_DB, 
	 Target.MDA_U0CN_VALUE_IZM_DB = Source.MDA_U0CN_VALUE_IZM_DB, 
     Target.MDA_U0N_VALUE_IZM_DB = Source.MDA_U0N_VALUE_IZM_DB, 
	 Target.MDA_UFCN_VALUE_IZM_DB = Source.MDA_UFCN_VALUE_IZM_DB, 
	 Target.MDA_UFN_VALUE_IZM_DB = Source.MDA_UFN_VALUE_IZM_DB, 
	 Target.MDA_ES_VALUE_IZM_DB = Source.MDA_ES_VALUE_IZM_DB, 
	 Target.MDA_ECN_VALUE_IZM_MKV = Source.MDA_ECN_VALUE_IZM_MKV, 
     Target.MDA_EN_VALUE_IZM_MKV = Source.MDA_EN_VALUE_IZM_MKV, 
	 Target.MDA_U0CN_VALUE_IZM_MKV = Source.MDA_U0CN_VALUE_IZM_MKV, 
	 Target.MDA_U0N_VALUE_IZM_MKV = Source.MDA_U0N_VALUE_IZM_MKV, 
	 Target.MDA_UFCN_VALUE_IZM_MKV = Source.MDA_UFCN_VALUE_IZM_MKV, 
	 Target.MDA_UFN_VALUE_IZM_MKV = Source.MDA_UFN_VALUE_IZM_MKV, 
     Target.MDA_ES_VALUE_IZM_MKV = Source.MDA_ES_VALUE_IZM_MKV, 
	 Target.MDA_EGS_DB = Source.MDA_EGS_DB, 
	 Target.MDA_EGS_MKV = Source.MDA_EGS_MKV, 
	 Target.MDA_E = Source.MDA_E, 
	 Target.MDA_UF = Source.MDA_UF, 
	 Target.MDA_U0 = Source.MDA_U0, 
	 Target.MDA_EGS = Source.MDA_EGS, 
	 Target.MDA_KP = Source.MDA_KP, 
	 Target.MDA_KPS = Source.MDA_KPS, 
	 Target.MDA_RBW = Source.MDA_RBW, 
	 Target.MDA_RBW_UNIT_ID = Source.MDA_RBW_UNIT_ID, 
     Target.MDA_ANT_ID = Source.MDA_ANT_ID, 
	 Target.MDA_KA = Source.MDA_KA, 
	 Target.MDA_KF = Source.MDA_KF, 
	 Target.MDA_K0 = Source.MDA_K0, 
	 Target.MDA_F_BEGIN = Source.MDA_F_BEGIN, 
	 Target.MDA_F_END = Source.MDA_F_END, 
	 Target.MDA_ECN_BEGIN = Source.MDA_ECN_BEGIN, 
	 Target.MDA_EN_BEGIN = Source.MDA_EN_BEGIN, 
	 Target.MDA_ECN_END = Source.MDA_ECN_END, 
	 Target.MDA_EN_END = Source.MDA_EN_END, 
	 Target.MDA_ANGLE = Source.MDA_ANGLE

    WHEN NOT MATCHED THEN
    INSERT 
           ( MDA_MODE_ID, MDA_F, MDA_F_UNIT_ID, MDA_I, MDA_ECN_VALUE_IZM, MDA_EN_VALUE_IZM, MDA_U0CN_VALUE_IZM, MDA_U0N_VALUE_IZM, 
                         MDA_UFCN_VALUE_IZM, MDA_UFN_VALUE_IZM, MDA_ES_VALUE_IZM, MDA_ECN_VALUE_IZM_DB, MDA_EN_VALUE_IZM_DB, MDA_U0CN_VALUE_IZM_DB, 
                         MDA_U0N_VALUE_IZM_DB, MDA_UFCN_VALUE_IZM_DB, MDA_UFN_VALUE_IZM_DB, MDA_ES_VALUE_IZM_DB, MDA_ECN_VALUE_IZM_MKV, 
                         MDA_EN_VALUE_IZM_MKV, MDA_U0CN_VALUE_IZM_MKV, MDA_U0N_VALUE_IZM_MKV, MDA_UFCN_VALUE_IZM_MKV, MDA_UFN_VALUE_IZM_MKV, 
                         MDA_ES_VALUE_IZM_MKV, MDA_EGS_DB, MDA_EGS_MKV, MDA_E, MDA_UF, MDA_U0, MDA_EGS, MDA_KP, MDA_KPS, MDA_RBW, MDA_RBW_UNIT_ID, 
                         MDA_ANT_ID,MDA_KA, MDA_KF, MDA_K0, MDA_F_BEGIN, MDA_F_END, MDA_ECN_BEGIN, MDA_EN_BEGIN, MDA_ECN_END, MDA_EN_END, MDA_ANGLE) 
    VALUES 
           ( Source.MDA_MODE_ID, Source.MDA_F, Source.MDA_F_UNIT_ID, Source.MDA_I, Source.MDA_ECN_VALUE_IZM, Source.MDA_EN_VALUE_IZM, Source.MDA_U0CN_VALUE_IZM, Source.MDA_U0N_VALUE_IZM, 
             Source.MDA_UFCN_VALUE_IZM, Source.MDA_UFN_VALUE_IZM, Source.MDA_ES_VALUE_IZM, Source.MDA_ECN_VALUE_IZM_DB, Source.MDA_EN_VALUE_IZM_DB, Source.MDA_U0CN_VALUE_IZM_DB, 
             Source.MDA_U0N_VALUE_IZM_DB, Source.MDA_UFCN_VALUE_IZM_DB, Source.MDA_UFN_VALUE_IZM_DB, Source.MDA_ES_VALUE_IZM_DB, Source.MDA_ECN_VALUE_IZM_MKV, 
             Source.MDA_EN_VALUE_IZM_MKV, Source.MDA_U0CN_VALUE_IZM_MKV, Source.MDA_U0N_VALUE_IZM_MKV, Source.MDA_UFCN_VALUE_IZM_MKV, Source.MDA_UFN_VALUE_IZM_MKV, 
             Source.MDA_ES_VALUE_IZM_MKV, Source.MDA_EGS_DB, Source.MDA_EGS_MKV, Source.MDA_E, MDA_UF, Source.MDA_U0, Source.MDA_EGS, Source.MDA_KP, Source.MDA_KPS, Source.MDA_RBW, Source.MDA_RBW_UNIT_ID, 
             Source.MDA_ANT_ID,Source.MDA_KA, MDA_KF, Source.MDA_K0, Source.MDA_F_BEGIN, Source.MDA_F_END, Source.MDA_ECN_BEGIN, Source.MDA_EN_BEGIN, Source.MDA_ECN_END, Source.MDA_EN_END, Source.MDA_ANGLE);
END