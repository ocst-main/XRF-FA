IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'HMTS2')
  BEGIN
    CREATE DATABASE [HMTS2]
  END
GO
USE [HMTS2]
GO

-- HMTS2.TB_XRF_SEQ_TEMP
SELECT 
	'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(k.parent_object_id) +
	'.[' + OBJECT_NAME(k.parent_object_id) + 
	'] DROP CONSTRAINT ' + k.name
	FROM sys.foreign_keys k
	WHERE referenced_object_id = object_id('TB_XRF_SEQ_TEMP');

IF EXISTS(SELECT * FROM sys.tables WHERE SCHEMA_NAME(schema_id) LIKE 'dbo' AND name like 'TB_XRF_SEQ_TEMP')  
   DROP TABLE [dbo].[TB_XRF_SEQ_TEMP];

CREATE TABLE TB_XRF_SEQ_TEMP
(
  SMPLNO      NVARCHAR(12)                 NOT NULL,
  TMBDIV      NVARCHAR(1)                  NOT NULL,
  ORDSEQ      DECIMAL(3),
  MTCHECK     NVARCHAR(2),
  RECHECK     NVARCHAR(1),
  SUJI        NVARCHAR(2),
  SMPLLENGTH  DECIMAL(10),
  EXNAME      NVARCHAR(50)
)

ALTER TABLE TB_XRF_SEQ_TEMP ADD CONSTRAINT PK_TB_XRF_SEQ_TEMP
	PRIMARY KEY  (SMPLNO, TMBDIV)

-- TB_XRF_AUTO_HISTORY
SELECT 
'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(k.parent_object_id) +
'.[' + OBJECT_NAME(k.parent_object_id) + 
'] DROP CONSTRAINT ' + k.name
FROM sys.foreign_keys k
WHERE referenced_object_id = object_id('TB_XRF_AUTO_HISTORY')

IF EXISTS(SELECT * FROM sys.tables WHERE SCHEMA_NAME(schema_id) LIKE 'dbo' AND name like 'TB_XRF_AUTO_HISTORY')  
   DROP TABLE [dbo].[TB_XRF_AUTO_HISTORY];

CREATE TABLE TB_XRF_AUTO_HISTORY
(
  XRFHSEQ        DECIMAL(8)                      NOT NULL,
  SMPLNO         NVARCHAR(12)              NOT NULL,
  TMBDIV         NVARCHAR(1)               NOT NULL,
  ELEMENTNAME    NVARCHAR(50),
  ELEMENTVALUE   DECIMAL(5,1),
  FBDIV          NVARCHAR(1),
  WCDDIV         NVARCHAR(1),
  XRFDATE        DATE,
  EXNAME         NVARCHAR(50),
  RESOURCEVALUE  DECIMAL(5,1)
)


ALTER TABLE TB_XRF_AUTO_HISTORY ADD CONSTRAINT PK_TB_XRF_AUTO_HISTORY2PRIMARY
	PRIMARY KEY (XRFHSEQ)

-- TB_XRF_FA_DIRECT
SELECT
	'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(k.parent_object_id) +
	'.[' + OBJECT_NAME(k.parent_object_id) + 
	'] DROP CONSTRAINT ' + k.name
	FROM sys.foreign_keys k
	WHERE referenced_object_id = object_id('TB_XRF_FA_DIRECT');

IF EXISTS(SELECT * FROM sys.tables WHERE SCHEMA_NAME(schema_id) LIKE 'dbo' AND name like 'TB_XRF_FA_DIRECT')  
   DROP TABLE [dbo].[TB_XRF_FA_DIRECT];

CREATE TABLE TB_XRF_FA_DIRECT
(
  SMPLNO          NVARCHAR(8)              NOT NULL,
  TMBDIV          NVARCHAR(1)              NOT NULL,
  CARVENUMBER     NVARCHAR(1)              NOT NULL,
  SMPLLENGTH      DECIMAL(4)                     NOT NULL,
  SUJI            NVARCHAR(2),
  PROCESSDIV      NVARCHAR(1),
  LOADER          NVARCHAR(1),
  LOADER_DATE     DATE,
  PRCMOVE         NVARCHAR(1),
  PRCMOVE_DATE    DATE,
  PRESS           NVARCHAR(1),
  PRESS_DATE      DATE,
  CARVE           NVARCHAR(1),
  CARVE_DATE      DATE,
  CLEAN           NVARCHAR(1),
  CLEAN_DATE      DATE,
  DISCHARGE       NVARCHAR(1),
  DISCHARGE_DATE  DATE,
  BUFFER          NVARCHAR(1),
  BUFFER_DATE     DATE,
  XRF             NVARCHAR(1),
  XRF_DATE        DATE,
  COMPLETE_DATE   DATE,
  FA_DIRECRT_SEQ  DECIMAL(10)                    DEFAULT 1
)


ALTER TABLE TB_XRF_FA_DIRECT ADD CONSTRAINT TB_XRF_FA_DIRECT_PK PRIMARY KEY
  (SMPLNO, TMBDIV, CARVENUMBER);

-- TB_XRF_HISTORY
SELECT
	'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(k.parent_object_id) +
	'.[' + OBJECT_NAME(k.parent_object_id) + 
	'] DROP CONSTRAINT ' + k.name
	FROM sys.foreign_keys k
	WHERE referenced_object_id = object_id('TB_XRF_HISTORY')

IF EXISTS(SELECT * FROM sys.tables WHERE SCHEMA_NAME(schema_id) LIKE 'dbo' AND name like 'TB_XRF_HISTORY')  
   DROP TABLE [dbo].[TB_XRF_HISTORY];

CREATE TABLE TB_XRF_HISTORY
(
  XRFHSEQ     DECIMAL(8)                         NOT NULL,
  SMPLNO      NVARCHAR(12)                 NOT NULL,
  TMBDIV      NVARCHAR(1)                  NOT NULL,
  SMPLLENGTH  DECIMAL(10),
  XRFFRONTW   DECIMAL(4,1),
  XRFFRONTC   DECIMAL(4,1),
  XRFFRONTD   DECIMAL(4,1),
  XRFFRONTA   DECIMAL(4,1),
  XRFBACKW    DECIMAL(4,1),
  XRFBACKC    DECIMAL(4,1),
  XRFBACKD    DECIMAL(4,1),
  XRFBACKA    DECIMAL(4,1),
  CRFRONTW    DECIMAL(4),
  CRFRONTC    DECIMAL(4),
  CRFRONTD    DECIMAL(4),
  CRFRONTA    DECIMAL(4),
  CRBACKW     DECIMAL(4),
  CRBACKC     DECIMAL(4),
  CRBACKD     DECIMAL(4),
  CRBACKA     DECIMAL(4),
  SIFRONTW    DECIMAL(4),
  SIFRONTC    DECIMAL(4),
  SIFRONTD    DECIMAL(4),
  SIFRONTA    DECIMAL(4),
  SIBACKW     DECIMAL(4),
  SIBACKC     DECIMAL(4),
  SIBACKD     DECIMAL(4),
  SIBACKA     DECIMAL(4),
  RSNFRONTW   DECIMAL(4),
  RSNFRONTC   DECIMAL(4),
  RSNFRONTD   DECIMAL(4),
  RSNFRONTA   DECIMAL(4),
  RSNBACKW    DECIMAL(4),
  RSNBACKC    DECIMAL(4),
  RSNBACKD    DECIMAL(4),
  RSNBACKA    DECIMAL(4),
  EXT1FRONTW  DECIMAL(4),
  EXT1FRONTC  DECIMAL(4),
  EXT1FRONTD  DECIMAL(4),
  EXT1FRONTA  DECIMAL(4),
  EXT1BACKW   DECIMAL(4),
  EXT1BACKC   DECIMAL(4),
  EXT1BACKD   DECIMAL(4),
  EXT1BACKA   DECIMAL(4),
  EXT2FRONTW  DECIMAL(4),
  EXT2FRONTC  DECIMAL(4),
  EXT2FRONTD  DECIMAL(4),
  EXT2FRONTA  DECIMAL(4),
  EXT2BACKW   DECIMAL(4),
  EXT2BACKC   DECIMAL(4),
  EXT2BACKD   DECIMAL(4),
  EXT2BACKA   DECIMAL(4),
  FEFRONTW    DECIMAL(3,1),
  FEFRONTC    DECIMAL(3,1),
  FEFRONTD    DECIMAL(3,1),
  FEFRONTA    DECIMAL(3,1),
  FEBACKW     DECIMAL(3,1),
  FEBACKC     DECIMAL(3,1),
  FEBACKD     DECIMAL(3,1),
  FEBACKA     DECIMAL(3,1),
  SUJIGUBUN   NVARCHAR(2),
  XRFAMOUNT   DECIMAL(4,1),
  XRFDATE     DATE
)

ALTER TABLE TB_XRF_HISTORY ADD CONSTRAINT PK_TB_XRF_HISTORY2 PRIMARY KEY (XRFHSEQ)

-- HMTS2.TB_CODE
SELECT
	'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(k.parent_object_id) +
	'.[' + OBJECT_NAME(k.parent_object_id) + 
	'] DROP CONSTRAINT ' + k.name
	FROM sys.foreign_keys k
	WHERE referenced_object_id = object_id('TB_CODE')

IF EXISTS(SELECT * FROM sys.tables WHERE SCHEMA_NAME(schema_id) LIKE 'dbo' AND name like 'TB_CODE')  
   DROP TABLE [dbo].[TB_CODE];

CREATE TABLE TB_CODE
(
  CODESEQ     DECIMAL(8)                         NOT NULL,
  CODEID      DECIMAL(5)                         NOT NULL,
  CODENAME    NVARCHAR(50),
  CODEVALUE   NVARCHAR(20),
  CODEMEAN    NVARCHAR(100),
  CREATEDATE  DATE,
  CODESORT    DECIMAL(4)                         DEFAULT 0
)

-- HMTS2.TB_XRF_APPLICATION_DETAIL =====================================================================================
SELECT
	'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(k.parent_object_id) +
	'.[' + OBJECT_NAME(k.parent_object_id) + 
	'] DROP CONSTRAINT ' + k.name
	FROM sys.foreign_keys k
	WHERE referenced_object_id = object_id('TB_XRF_APPLICATION_DETAIL')

IF EXISTS(SELECT * FROM sys.tables WHERE SCHEMA_NAME(schema_id) LIKE 'dbo' AND name like 'TB_XRF_APPLICATION_DETAIL')  
   DROP TABLE [dbo].[TB_XRF_APPLICATION_DETAIL];

CREATE TABLE TB_XRF_APPLICATION_DETAIL
(
  FACTORY          NVARCHAR(4)             NOT NULL,
  LINENAME         NVARCHAR(2)             NOT NULL,
  COILNAME         NVARCHAR(2)             NOT NULL,
  SHTSPEC          NVARCHAR(20)            DEFAULT '--'                  NOT NULL,
  AFTCODE          NVARCHAR(2)             NOT NULL,
  ELEMENT          NVARCHAR(20)            NOT NULL,
  RELOPERATORONE   NVARCHAR(10),
  RELVALUEONE      NVARCHAR(100),
  LOGICALOPERATOR  NVARCHAR(10),
  RELOPERATORTWO   NVARCHAR(10),
  RELVALUETWO      NVARCHAR(100),
  POINTSHOW        NVARCHAR(10),
  LEST             NVARCHAR(10),
  COMPUTEMODIFY    NVARCHAR(100),
  DETAILSEQ        DECIMAL(10)                   NOT NULL,
  APPNAME          NVARCHAR(50)
)

-- HMTS2.TB_XRF_APPLICATION_NAME =====================================================================================
SELECT
	'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(k.parent_object_id) +
	'.[' + OBJECT_NAME(k.parent_object_id) + 
	'] DROP CONSTRAINT ' + k.name
	FROM sys.foreign_keys k
	WHERE referenced_object_id = object_id('TB_XRF_APPLICATION_NAME')

IF EXISTS(SELECT * FROM sys.tables WHERE SCHEMA_NAME(schema_id) LIKE 'dbo' AND name like 'TB_XRF_APPLICATION_NAME')  
   DROP TABLE [dbo].[TB_XRF_APPLICATION_NAME];

CREATE TABLE TB_XRF_APPLICATION_NAME
(
  FACTORY       NVARCHAR(4)                DEFAULT 'N'                   NOT NULL,
  LINENAME      NVARCHAR(2)                NOT NULL,
  COILNAME      NVARCHAR(2)                NOT NULL,
  SHTSPEC       NVARCHAR(20)               DEFAULT '--'                  NOT NULL,
  AFTCODE       NVARCHAR(2)                NOT NULL,
  APPNAME       NVARCHAR(50)               DEFAULT NULL                  NOT NULL,
  DEFAULTCHECK  NVARCHAR(1)                DEFAULT 'N'
)

-- HMTS2.TB_XRF_SEQ =====================================================================================
SELECT
	'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(k.parent_object_id) +
	'.[' + OBJECT_NAME(k.parent_object_id) + 
	'] DROP CONSTRAINT ' + k.name
	FROM sys.foreign_keys k
	WHERE referenced_object_id = object_id('TB_XRF_SEQ')

IF EXISTS(SELECT * FROM sys.tables WHERE SCHEMA_NAME(schema_id) LIKE 'dbo' AND name like 'TB_XRF_SEQ')  
   DROP TABLE [dbo].[TB_XRF_SEQ];

CREATE TABLE TB_XRF_SEQ
(
  SMPLNO      NVARCHAR(12)                 NOT NULL,
  TMBDIV      NVARCHAR(1)                  NOT NULL,
  ORDSEQ      DECIMAL(3)                         NOT NULL,
  MTCHECK     NVARCHAR(2)                  NOT NULL,
  RECHECK     NVARCHAR(1),
  SUJI        NVARCHAR(2),
  SMPLLENGTH  DECIMAL(10),
  EXNAME      NVARCHAR(50),
  CLEANYN     NVARCHAR(1)                  DEFAULT 'N',
  DIVTYPE     NVARCHAR(20)                 DEFAULT 'WCD'
)

-- HMTS2.TB_TEST_DIRECT =====================================================================================
SELECT
	'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(k.parent_object_id) +
	'.[' + OBJECT_NAME(k.parent_object_id) + 
	'] DROP CONSTRAINT ' + k.name
	FROM sys.foreign_keys k
	WHERE referenced_object_id = object_id('TB_TEST_DIRECT')

IF EXISTS(SELECT * FROM sys.tables WHERE SCHEMA_NAME(schema_id) LIKE 'dbo' AND name like 'TB_TEST_DIRECT')  
   DROP TABLE [dbo].[TB_TEST_DIRECT];

CREATE TABLE TB_TEST_DIRECT
(
  SMPLNO           NVARCHAR(12)            NOT NULL,
  TMBDIV           NVARCHAR(1)             NOT NULL,
  TESTORGAN        NVARCHAR(3),
  COILNAME         NVARCHAR(2),
  LINENAME         NVARCHAR(2),
  SHTSPEC          NVARCHAR(20),
  QUALCODE         NVARCHAR(8),
  ORDERCOILHGT     DECIMAL(5,3),
  ORDERCOILWDT     DECIMAL(5,1),
  COILHGT          DECIMAL(5,3),
  COILWDT          DECIMAL(5,1),
  ORDNO            NVARCHAR(10),
  ORDSEQ           NVARCHAR(3),
  DELIVDATE        DATE,
  CUSTOMERCD       NVARCHAR(5),
  SALECD           NVARCHAR(6),
  AFTCODE          NVARCHAR(2),
  USECD            NVARCHAR(4),
  ORIGINMK         NVARCHAR(3),
  MTDATE           DATE,
  MDATE            DATE,
  TSSMPL           NVARCHAR(7),
  TSRST            NVARCHAR(1),
  TSMIN            DECIMAL(4,1),
  TSMAX            DECIMAL(4,1),
  YPRST            NVARCHAR(1),
  YPMIN            DECIMAL(4,1),
  YPMAX            DECIMAL(4,1),
  ELRST            NVARCHAR(1),
  ELMIN            DECIMAL(2),
  ELMAX            DECIMAL(2),
  YRRST            NVARCHAR(1),
  YRMIN            DECIMAL(3,1),
  YRMAX            DECIMAL(3,1),
  YPELRST          NVARCHAR(1),
  YPELMAX          DECIMAL(3,1),
  NRST             NVARCHAR(1),
  NMIN             DECIMAL(4,3),
  NMAX             DECIMAL(4,3),
  AIRST            NVARCHAR(1),
  AIMIN            DECIMAL(3,1),
  AIMAX            DECIMAL(3,1),
  BHRST            NVARCHAR(1),
  BHMIN            DECIMAL(3,1),
  BHMAX            DECIMAL(3,1),
  RARST            NVARCHAR(1),
  RAMIN            DECIMAL(3,2),
  RAMAX            DECIMAL(3,2),
  RMAXRST          NVARCHAR(1),
  RMAXMIN          DECIMAL(4,2),
  RMAXMAX          DECIMAL(4,2),
  PPIRST           NVARCHAR(1),
  PPIMIN           DECIMAL(3),
  HARDRST          NVARCHAR(1),
  HARDMIN          DECIMAL(2),
  HARDMAX          DECIMAL(2),
  XRFRST           NVARCHAR(1),
  XRFFRONTMIN      DECIMAL(4,1),
  XRFFRONTMAX      DECIMAL(4,1),
  XRFBACKMIN       DECIMAL(4,1),
  XRFBACKMAX       DECIMAL(4,1),
  FERST            NVARCHAR(1),
  FEFRONTMIN       DECIMAL(3,1),
  FEFRONTMAX       DECIMAL(3,1),
  FEBACKMIN        DECIMAL(3,1),
  FEBACKMAX        DECIMAL(3,1),
  CRRST            NVARCHAR(1),
  CRFRONTMIN       DECIMAL(4),
  CRFRONTMAX       DECIMAL(4),
  CRBACKMIN        DECIMAL(4),
  CRBACKMAX        DECIMAL(4),
  SIRST            NVARCHAR(1),
  SIFRONTMIN       DECIMAL(4),
  SIFRONTMAX       DECIMAL(4),
  SIBACKMIN        DECIMAL(4),
  SIBACKMAX        DECIMAL(4),
  RSNRST           NVARCHAR(1),
  RSNFRONTMIN      DECIMAL(4),
  RSNFRONTMAX      DECIMAL(4),
  RSNBACKMIN       DECIMAL(4),
  RSNBACKMAX       DECIMAL(4),
  EXT1RST          NVARCHAR(1),
  EXT1FRONTMIN     DECIMAL(4),
  EXT1FRONTMAX     DECIMAL(4),
  EXT1BACKMIN      DECIMAL(4),
  EXT1BACKMAX      DECIMAL(4),
  EXT2RST          NVARCHAR(1),
  EXT2FRONTMIN     DECIMAL(4),
  EXT2FRONTMAX     DECIMAL(4),
  EXT2BACKMIN      DECIMAL(4),
  EXT2BACKMAX      DECIMAL(4),
  RBARRST          NVARCHAR(1),
  RBARMIN          DECIMAL(3,2),
  R90CHECK         NVARCHAR(1),
  R90RST           NVARCHAR(1),
  R90MIN           DECIMAL(3,2),
  DELTARRST        NVARCHAR(1),
  DELTARSIGN       NVARCHAR(1),
  DELTARMIN        DECIMAL(5,3),
  DELTARMAX        DECIMAL(5,3),
  UTMCHECK         NVARCHAR(1),
  UTMSTATUS        NVARCHAR(1),
  AICHECK          NVARCHAR(1),
  AISTATUS         NVARCHAR(1),
  BHCHECK          NVARCHAR(1),
  BHSTATUS         NVARCHAR(1),
  ROUGHCHECK       NVARCHAR(1),
  ROUGHSTATUS      NVARCHAR(1),
  HARDCHECK        NVARCHAR(1),
  HARDSTATUS       NVARCHAR(1),
  XRFCHECK         NVARCHAR(1),
  XRFSTATUS        NVARCHAR(1),
  DRAWCHECK        NVARCHAR(1),
  DRAWSTATUS       NVARCHAR(1),
  DIRECTCODE       NVARCHAR(2),
  BBENDCHECK       NVARCHAR(1),
  BBENDRST         NVARCHAR(1),
  BBENDSTAN        NVARCHAR(3),
  CBENDCHECK       NVARCHAR(1),
  CBENDRST         NVARCHAR(1),
  CBENDSTAN        NVARCHAR(3),
  PTCHECK          NVARCHAR(1),
  PTRST            NVARCHAR(1),
  PTSTAN           NVARCHAR(1),
  SUMAKRST         NVARCHAR(1),
  SUMAKSTAN        NVARCHAR(1),
  GULRST           NVARCHAR(1),
  GULSTAN          NVARCHAR(1),
  QUALSIGN         NVARCHAR(6),
  WCDCHECK         NVARCHAR(1),
  WCDRST           NVARCHAR(1),
  WCDMIX           DECIMAL(3,1),
  WCDMAX           DECIMAL(3,1),
  OESCHECK         NVARCHAR(1),
  TEMP1CHECK       NVARCHAR(1),
  TEMP2CHECK       NVARCHAR(1),
  IMPORTCHECK      NVARCHAR(1),
  OESSTATUS        NVARCHAR(1),
  WCDSTATUS        NVARCHAR(1),
  IMPORTDATE       DATE,
  YPRTESTCHECK     NVARCHAR(1),
  TSRTESTCHECK     NVARCHAR(1),
  ELRTESTCHECK     NVARCHAR(1),
  YPELRTESTCHECK   NVARCHAR(1),
  NVALRTESTCHECK   NVARCHAR(1),
  R90RTESTCHECK    NVARCHAR(1),
  BHRTESTCHECK     NVARCHAR(1),
  ATFRTESTCHECK    NVARCHAR(1),
  HRBRTESTCHECK    NVARCHAR(1),
  RRARTESTCHECK    NVARCHAR(1),
  RBARRTESTCHECK   NVARCHAR(1),
  BBNDRTESTCHECK   NVARCHAR(1),
  CBNDRTESTCHECK   NVARCHAR(1),
  PWDRTESTCHECK    NVARCHAR(1),
  FGALWRTESTCHECK  NVARCHAR(1),
  RATARTESTCHECK   NVARCHAR(1),
  FCAWRTESTCHECK   NVARCHAR(1),
  FRAWRTESTCHECK   NVARCHAR(1),
  WCDRTESTCHECK    NVARCHAR(1),
  MSGSEQNO         DECIMAL(2),
  UTMTSTESTCHECK   NVARCHAR(1),
  UTMRBARST        NVARCHAR(1),
  UTMRBARMIN       DECIMAL(3),
  UTMDELTARRST     NVARCHAR(1),
  UTMDELTARSIGN    NVARCHAR(1),
  UTMDELTARMIN     DECIMAL(3),
  UTMDELTARMAX     DECIMAL(3),
  UTMR90RST        NVARCHAR(1),
  UTMR90MIN        DECIMAL(3),
  TSSMPCD          NVARCHAR(1),
  YPTESTCD         NVARCHAR(4),
  RTESTCD          NVARCHAR(4),
  RMIN             DECIMAL(2),
  RMAX             DECIMAL(2),
  R2TESTCD         NVARCHAR(4),
  R2MIN            DECIMAL(2),
  R2MAX            DECIMAL(2),
  NTESTCD          NVARCHAR(4),
  NTSTMIN          DECIMAL(2),
  NTSTMAX          DECIMAL(2),
  BHTESTCD         NVARCHAR(4),
  ROUGHTSTCD       NVARCHAR(4),
  MAINLINENAME     NVARCHAR(2),
  UTMRCHECK        NVARCHAR(1),
  RBARSTATUS       NVARCHAR(1),
  UTMR0RST         NVARCHAR(1),
  UTMR0MIN         DECIMAL(3),
  UTMR0CHECK       NVARCHAR(1),
  UTMR0STATUS      NVARCHAR(1),
  UTMR45RST        NVARCHAR(1),
  UTMR45MIN        DECIMAL(3),
  UTMR45CHECK      NVARCHAR(1),
  UTMR45STATUS     NVARCHAR(1),
  TESTGALTVAL      DECIMAL(4,3),
  TESTGALTCODE     NVARCHAR(1),
  RTESTGATVAL      DECIMAL(4,3),
  NBARRST          NVARCHAR(1),
  NBARMIN          DECIMAL(3,2),
  NBARMAX          DECIMAL(3,2),
  ROCHECK          NVARCHAR(1),
  YELLOWRCHECK     NVARCHAR(1),
  GULCHECK         NVARCHAR(1),
  HOLECHECK        NVARCHAR(1),
  NBARCHECK        NVARCHAR(1),
  SAMPLECODE       NVARCHAR(21),
  NBARSTATUS       NVARCHAR(1),
  PTSTATUS         NVARCHAR(1),
  OVSTATUS         NVARCHAR(1),
  OVCHECK          NVARCHAR(1),
  GALSMPCD         NVARCHAR(7),
  ROUVALLTH        DECIMAL(1),
  THICKSTATUS      NVARCHAR(1),
  THCHECK          NVARCHAR(1),
  CBENDSTATUS      NVARCHAR(1),
  THRCHECK         NVARCHAR(1),
  THICKMIN         DECIMAL(5,3),
  THICKMAX         DECIMAL(5,3),
  NVALCD           DECIMAL(1),
  BBENDSPEC        NVARCHAR(4),
  CBENDSPEC        NVARCHAR(4),
  SPMUSE           NVARCHAR(1),
  PWDRSTVAL        DECIMAL(4,2),
  RPCRST           NVARCHAR(1),
  RRPCRTESTCHECK   NVARCHAR(1),
  RPCMAX           DECIMAL(5,2),
  RPCMIN           DECIMAL(5,2)
)

-- HMTS2.TB_TEST_RESULT =====================================================================================
SELECT
	'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(k.parent_object_id) +
	'.[' + OBJECT_NAME(k.parent_object_id) + 
	'] DROP CONSTRAINT ' + k.name
	FROM sys.foreign_keys k
	WHERE referenced_object_id = object_id('TB_TEST_RESULT')

IF EXISTS(SELECT * FROM sys.tables WHERE SCHEMA_NAME(schema_id) LIKE 'dbo' AND name like 'TB_TEST_RESULT')  
   DROP TABLE [dbo].[TB_TEST_RESULT];

CREATE TABLE TB_TEST_RESULT
(
  SMPLNO           NVARCHAR(12)            NOT NULL,
  TMBDIV           NVARCHAR(1)             NOT NULL,
  SMPLHGT          DECIMAL(5,3),
  TSVAL            DECIMAL(4,1),
  YPVAL            DECIMAL(4,1),
  ELVAL            DECIMAL(2),
  YRVAL            DECIMAL(3,1),
  YPELVAL          DECIMAL(3,1),
  NVAL             DECIMAL(4,3),
  RVAL             DECIMAL(3,2),
  YP02VAL          DECIMAL(4,1),
  AIVAL            DECIMAL(3,1),
  BHVAL            DECIMAL(3,1),
  RAFRONTVAL       DECIMAL(3,2),
  RABACKVAL        DECIMAL(3,2),
  RMAXFRONTVAL     DECIMAL(4,2),
  RMAXBACKVAL      DECIMAL(4,2),
  PPIFRONTVAL      DECIMAL(3),
  PPIBACKVAL       DECIMAL(3),
  HRBVAL           DECIMAL(4,1),
  XRFFRONTW        DECIMAL(4,1),
  XRFFRONTC        DECIMAL(4,1),
  XRFFRONTD        DECIMAL(4,1),
  XRFFRONTA        DECIMAL(4,1),
  XRFBACKW         DECIMAL(4,1),
  XRFBACKC         DECIMAL(4,1),
  XRFBACKD         DECIMAL(4,1),
  XRFBACKA         DECIMAL(4,1),
  XRFAMOUNT        DECIMAL(4,1),
  CRFRONTW         DECIMAL(4),
  CRFRONTC         DECIMAL(4),
  CRFRONTD         DECIMAL(4),
  CRFRONTA         DECIMAL(4),
  CRBACKW          DECIMAL(4),
  CRBACKC          DECIMAL(4),
  CRBACKD          DECIMAL(4),
  CRBACKA          DECIMAL(4),
  SIFRONTW         DECIMAL(4),
  SIFRONTC         DECIMAL(4),
  SIFRONTD         DECIMAL(4),
  SIFRONTA         DECIMAL(4),
  SIBACKW          DECIMAL(4),
  SIBACKC          DECIMAL(4),
  SIBACKD          DECIMAL(4),
  SIBACKA          DECIMAL(4),
  RSNFRONTW        DECIMAL(4),
  RSNFRONTC        DECIMAL(4),
  RSNFRONTD        DECIMAL(4),
  RSNFRONTA        DECIMAL(4),
  RSNBACKW         DECIMAL(4),
  RSNBACKC         DECIMAL(4),
  RSNBACKD         DECIMAL(4),
  RSNBACKA         DECIMAL(4),
  EXT1FRONTW       DECIMAL(4),
  EXT1FRONTC       DECIMAL(4),
  EXT1FRONTD       DECIMAL(4),
  EXT1FRONTA       DECIMAL(4),
  EXT1BACKW        DECIMAL(4),
  EXT1BACKC        DECIMAL(4),
  EXT1BACKD        DECIMAL(4),
  EXT1BACKA        DECIMAL(4),
  EXT2FRONTW       DECIMAL(4),
  EXT2FRONTC       DECIMAL(4),
  EXT2FRONTD       DECIMAL(4),
  EXT2FRONTA       DECIMAL(4),
  EXT2BACKW        DECIMAL(4),
  EXT2BACKC        DECIMAL(4),
  EXT2BACKD        DECIMAL(4),
  EXT2BACKA        DECIMAL(4),
  FEFRONTW         DECIMAL(3,1),
  FEFRONTC         DECIMAL(3,1),
  FEFRONTD         DECIMAL(3,1),
  FEFRONTA         DECIMAL(3,1),
  FEBACKW          DECIMAL(3,1),
  FEBACKC          DECIMAL(3,1),
  FEBACKD          DECIMAL(3,1),
  FEBACKA          DECIMAL(3,1),
  DEGREEFRE0       DECIMAL(4,2),
  DEGREEFRE45      DECIMAL(4,2),
  DEGREEFRE90      DECIMAL(4,2),
  R0VAL            DECIMAL(4,2),
  R45VAL           DECIMAL(4,2),
  R90VAL           DECIMAL(4,2),
  DRAWRBARVAL      DECIMAL(3,2),
  DRAWDELTARVAL    DECIMAL(3,2),
  RESULTSEND       NVARCHAR(1),
  UTMJUDGE         NVARCHAR(1),
  AIJUDGE          NVARCHAR(1),
  BHJUDGE          NVARCHAR(1),
  XRFJUDGE         NVARCHAR(1),
  ROUGHJUDGE       NVARCHAR(1),
  HARDJUDGE        NVARCHAR(1),
  DRAWJUDGE        NVARCHAR(1),
  WCDJUDGE         NVARCHAR(1),
  SUJIGUBUN        NVARCHAR(2),
  LINENAME         NVARCHAR(2),
  BBENDVAL         NVARCHAR(1),
  CBENDVAL         NVARCHAR(1),
  PTVAL            NVARCHAR(1),
  BIVAL            NVARCHAR(1),
  SUMAKVAL         NVARCHAR(1),
  GULVAL           NVARCHAR(1),
  AIBHSMPLHGT      DECIMAL(5,3),
  AIBHSMPLWDT      DECIMAL(4,2),
  YPLFVAL          DECIMAL(4,1),
  YPLSVAL          DECIMAL(4,1),
  SENDER           NVARCHAR(8),
  WCDVAL           DECIMAL(3,1),
  WCDMIN           DECIMAL(3,1),
  WCDMAX           DECIMAL(3,1),
  OESALVAL         DECIMAL(6,4),
  OESBVAL          DECIMAL(6,4),
  OESCVAL          DECIMAL(6,4),
  OESCOVAL         DECIMAL(6,4),
  OESCRVAL         DECIMAL(6,4),
  OESCUVAL         DECIMAL(6,4),
  OESFEVAL         DECIMAL(6,4),
  OESMNVAL         DECIMAL(6,4),
  OESMOVAL         DECIMAL(6,4),
  OESNBVAL         DECIMAL(6,4),
  OESPVAL          DECIMAL(6,4),
  OESPBVAL         DECIMAL(6,4),
  OESSVAL          DECIMAL(6,4),
  OESSIVAL         DECIMAL(6,4),
  OESSNVAL         DECIMAL(6,4),
  OESTIVAL         DECIMAL(6,4),
  OESVVAL          DECIMAL(6,4),
  OESWVAL          DECIMAL(6,4),
  OESZRVAL         DECIMAL(6,4),
  OESEXT1VAL       DECIMAL(6,4),
  OESEXT2VAL       DECIMAL(6,4),
  OESEXT3VAL       DECIMAL(6,4),
  OESEXT4VAL       DECIMAL(6,4),
  OESEXT5VAL       DECIMAL(6,4),
  OESNIVAL         DECIMAL(6,4),
  CUTCOMPLETE      NVARCHAR(1),
  UTMTESTDATE      DATE,
  UTMCODE          NVARCHAR(20),
  SMPLWDT          DECIMAL(5,2),
  FAIBHCODE        NVARCHAR(2),
  FAIBHTESTDATE    DATE,
  SAIBHCODE        NVARCHAR(2),
  SAIBHTESTDATE    DATE,
  YPELSVAL         DECIMAL(3,1),
  YPELFVAL         DECIMAL(3,1),
  TSFVAL           DECIMAL(4,1),
  TSSVAL           DECIMAL(4,1),
  YPSVAL           DECIMAL(4,1),
  YPFVAL           DECIMAL(4,1),
  YP02SVAL         DECIMAL(4,1),
  YP02FVAL         DECIMAL(4,1),
  HRBDATE          DATE,
  DRAWABILITYDATE  DATE,
  HARDDIVCODE      NVARCHAR(20),
  DRAWDIVCODE      NVARCHAR(20),
  SUMAKDATE        DATE,
  PTDATE           DATE,
  CBENDDATE        DATE,
  BBENDDATE        DATE,
  ROUGHDATE        DATE,
  XRFTESTDATE      DATE,
  SENDPC           NVARCHAR(8),
  RZBACKVAL        DECIMAL(3,2),
  RZFRONTVAL       DECIMAL(3,2),
  MODIFYDATE       DATE,
  WCDTESTDATE      DATE,
  OESTESTDATE      DATE,
  WETTEST          NVARCHAR(1),
  YPRTESTCHECK     NVARCHAR(1),
  TSRTESTCHECK     NVARCHAR(1),
  ELRTESTCHECK     NVARCHAR(1),
  YPELRTESTCHECK   NVARCHAR(1),
  NVALRTESTCHECK   NVARCHAR(1),
  R90RTESTCHECK    NVARCHAR(1),
  BHRTESTCHECK     NVARCHAR(1),
  ATFRTESTCHECK    NVARCHAR(1),
  HRBRTESTCHECK    NVARCHAR(1),
  RRARTESTCHECK    NVARCHAR(1),
  RBARRTESTCHECK   NVARCHAR(1),
  BBNDRTESTCHECK   NVARCHAR(1),
  CBNDRTESTCHECK   NVARCHAR(1),
  PWDRTESTCHECK    NVARCHAR(1),
  FGALWRTESTCHECK  NVARCHAR(1),
  RATARTESTCHECK   NVARCHAR(1),
  FCAWRTESTCHECK   NVARCHAR(1),
  FRAWRTESTCHECK   NVARCHAR(1),
  WCDRTESTCHECK    NVARCHAR(1),
  MSGSEQNO         DECIMAL(2),
  UELVAL           DECIMAL(3,1),
  PELVAL           DECIMAL(3,1),
  RPCFRONTVAL      DECIMAL(5),
  RPCBACKVAL       DECIMAL(5),
  WA               DECIMAL(6,3),
  WSA              DECIMAL(6,3),
  UTMR0            DECIMAL(4,3),
  UTMR45           DECIMAL(4,3),
  UTMR90           DECIMAL(4,3),
  RPCTOP           DECIMAL(5),
  RPCBOT           DECIMAL(5),
  POFRONTW         DECIMAL(2,1),
  POBACKW          DECIMAL(2,1),
  POFRONTRST       DECIMAL(1),
  POBACKRST        DECIMAL(1),
  POFRONTRSTGN     NVARCHAR(1),
  POBACKRSTGN      NVARCHAR(1),
  SXRFFRONTW       DECIMAL(4,1),
  SXRFFRONTC       DECIMAL(4,1),
  SXRFFRONTD       DECIMAL(4,1),
  SXRFFRONTA       DECIMAL(4,1),
  SXRFBACKW        DECIMAL(4,1),
  SXRFBACKC        DECIMAL(4,1),
  SXRFBACKD        DECIMAL(4,1),
  SXRFBACKA        DECIMAL(4,1),
  SXRFAMOUNT       DECIMAL(4,1),
  UTMN0            DECIMAL(4,2),
  UTMN45           DECIMAL(4,2),
  UTMN90           DECIMAL(4,2),
  NBARVAL          DECIMAL(4,2),
  BHVALUE          DECIMAL(5,2),
  UTMBREAKIO       NVARCHAR(5),
  ROCHECK          NVARCHAR(1),
  YELLOWRCHECK     NVARCHAR(1),
  GULCHECK         NVARCHAR(1),
  HOLECHECK        NVARCHAR(1),
  NBARCHECK        NVARCHAR(1),
  HRBVAL1          DECIMAL(4,1),
  HRBVAL2          DECIMAL(4,1),
  HRBVAL3          DECIMAL(4,1),
  POFRONTW1        DECIMAL(3,1),
  POBACKW1         DECIMAL(3,1),
  OVENMIN          DECIMAL(4,1),
  OVENMAX          DECIMAL(4,1),
  OVENAVE          DECIMAL(4,1),
  OVENHEAT         DECIMAL(4),
  OVENDATE         DATE,
  GALFRN1          DECIMAL(1),
  GALFRN2          DECIMAL(1),
  GALFRN3          DECIMAL(1),
  GALFRN4          DECIMAL(1),
  GALFRN5          DECIMAL(1),
  GALFRN6          DECIMAL(1),
  GALFRN7          DECIMAL(1),
  GALFRN8          DECIMAL(1),
  GALFRN9          DECIMAL(1),
  GALFRN10         DECIMAL(1),
  GALBAK1          DECIMAL(1),
  GALBAK2          DECIMAL(1),
  GALBAK3          DECIMAL(1),
  GALBAK4          DECIMAL(1),
  GALBAK5          DECIMAL(1),
  GALBAK6          DECIMAL(1),
  GALBAK7          DECIMAL(1),
  GALBAK8          DECIMAL(1),
  GALBAK9          DECIMAL(1),
  GALBAK10         DECIMAL(1),
  MINTH            DECIMAL(5,3),
  MAXTH            DECIMAL(5,3),
  AVETH            DECIMAL(5,3),
  LEFTTH           DECIMAL(5,3),
  RIGHTTH          DECIMAL(5,3),
  THICKDATE        DATE,
  ONETHICK         DECIMAL(5,3),
  CENTERTHICK      DECIMAL(5,3),
  THREETHICK       DECIMAL(5,3),
  LEFT40TH         DECIMAL(5,3),
  RIGHT40TH        DECIMAL(5,3),
  GALFRN11         DECIMAL(1),
  GALFRN12         DECIMAL(1),
  GALFRN13         DECIMAL(1),
  GALFRN14         DECIMAL(1),
  GALFRN15         DECIMAL(1),
  GALBAK11         DECIMAL(1),
  GALBAK12         DECIMAL(1),
  GALBAK13         DECIMAL(1),
  GALBAK14         DECIMAL(1),
  GALBAK15         DECIMAL(1),
  THRCHECK         NVARCHAR(1),
  ELVAL1           DECIMAL(4,1),
  BHVAL1           DECIMAL(6,2),
  XRFMT            NVARCHAR(2),
  BBFVAL           DECIMAL(1),
  BBBVAL           DECIMAL(1),
  RRPCRTESTCHECK   NVARCHAR(1),
  YP05VAL          DECIMAL(4,1),
  YPHVAL           DECIMAL(4,1),
  YPLVAL           DECIMAL(4,1),
  YPTESTCDRESULT   NVARCHAR(1)
)

