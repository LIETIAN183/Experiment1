using System;
using System.Collections.Generic;
using BansheeGz.BGDatabase;

//=============================================================
//||                   Generated by BansheeGz Code Generator ||
//=============================================================

#pragma warning disable 414

//=============================================================
//||                   Generated by BansheeGz Code Generator ||
//=============================================================

public partial class DB_Detail : BGEntity
{

	//=============================================================
	//||                   Generated by BansheeGz Code Generator ||
	//=============================================================

	public class Factory : BGEntity.EntityFactory
	{
		public BGEntity NewEntity(BGMetaEntity meta)
		{
			return new DB_Detail(meta);
		}
		public BGEntity NewEntity(BGMetaEntity meta, BGId id)
		{
			return new DB_Detail(meta, id);
		}
	}
	private static BansheeGz.BGDatabase.BGMetaRow _metaDefault;
	public static BansheeGz.BGDatabase.BGMetaRow MetaDefault
	{
		get
		{
			if(_metaDefault==null || _metaDefault.IsDeleted) _metaDefault=BGRepo.I.GetMeta<BansheeGz.BGDatabase.BGMetaRow>(new BGId(5702248367193815549UL,10019676743514381222UL));
			return _metaDefault;
		}
	}
	public static BansheeGz.BGDatabase.BGRepoEvents Events
	{
		get
		{
			return BGRepo.I.Events;
		}
	}
	private static readonly List<BGEntity> _find_Entities_Result = new List<BGEntity>();
	public static int CountEntities
	{
		get
		{
			return MetaDefault.CountEntities;
		}
	}
	public System.String F_name
	{
		get
		{
			return _F_name[Index];
		}
		set
		{
			_F_name[Index] = value;
		}
	}
	public System.Int32 F_eqIndex
	{
		get
		{
			return _F_eqIndex[Index];
		}
		set
		{
			_F_eqIndex[Index] = value;
		}
	}
	public System.Single F_time
	{
		get
		{
			return _F_time[Index];
		}
		set
		{
			_F_time[Index] = value;
		}
	}
	public System.Single F_xAcc
	{
		get
		{
			return _F_xAcc[Index];
		}
		set
		{
			_F_xAcc[Index] = value;
		}
	}
	public System.Single F_zAcc
	{
		get
		{
			return _F_zAcc[Index];
		}
		set
		{
			_F_zAcc[Index] = value;
		}
	}
	public System.Single F_yAcc
	{
		get
		{
			return _F_yAcc[Index];
		}
		set
		{
			_F_yAcc[Index] = value;
		}
	}
	public System.Single F_horiAcc
	{
		get
		{
			return _F_horiAcc[Index];
		}
		set
		{
			_F_horiAcc[Index] = value;
		}
	}
	public System.Single F_Acc
	{
		get
		{
			return _F_Acc[Index];
		}
		set
		{
			_F_Acc[Index] = value;
		}
	}
	public System.Int32 F_dropCount
	{
		get
		{
			return _F_dropCount[Index];
		}
		set
		{
			_F_dropCount[Index] = value;
		}
	}
	public System.Int32 F_escaped
	{
		get
		{
			return _F_escaped[Index];
		}
		set
		{
			_F_escaped[Index] = value;
		}
	}
	public System.Int32 F_escapredPerSec
	{
		get
		{
			return _F_escapredPerSec[Index];
		}
		set
		{
			_F_escapredPerSec[Index] = value;
		}
	}
	public System.String F_GroupID
	{
		get
		{
			return _F_GroupID[Index];
		}
		set
		{
			_F_GroupID[Index] = value;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldEntityName _ufle12jhs77_F_name;
	public static BansheeGz.BGDatabase.BGFieldEntityName _F_name
	{
		get
		{
			if(_ufle12jhs77_F_name==null || _ufle12jhs77_F_name.IsDeleted) _ufle12jhs77_F_name=(BansheeGz.BGDatabase.BGFieldEntityName) MetaDefault.GetField(new BGId(4747034596549896910UL,85180650801044154UL));
			return _ufle12jhs77_F_name;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_F_eqIndex;
	public static BansheeGz.BGDatabase.BGFieldInt _F_eqIndex
	{
		get
		{
			if(_ufle12jhs77_F_eqIndex==null || _ufle12jhs77_F_eqIndex.IsDeleted) _ufle12jhs77_F_eqIndex=(BansheeGz.BGDatabase.BGFieldInt) MetaDefault.GetField(new BGId(5083433130941209019UL,11676197540464617879UL));
			return _ufle12jhs77_F_eqIndex;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_time;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_time
	{
		get
		{
			if(_ufle12jhs77_F_time==null || _ufle12jhs77_F_time.IsDeleted) _ufle12jhs77_F_time=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5048311899048495167UL,661230998669310342UL));
			return _ufle12jhs77_F_time;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_xAcc;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_xAcc
	{
		get
		{
			if(_ufle12jhs77_F_xAcc==null || _ufle12jhs77_F_xAcc.IsDeleted) _ufle12jhs77_F_xAcc=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5553463505686242826UL,10739730355220489606UL));
			return _ufle12jhs77_F_xAcc;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_zAcc;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_zAcc
	{
		get
		{
			if(_ufle12jhs77_F_zAcc==null || _ufle12jhs77_F_zAcc.IsDeleted) _ufle12jhs77_F_zAcc=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5604338849405908822UL,6501025683771727769UL));
			return _ufle12jhs77_F_zAcc;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_yAcc;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_yAcc
	{
		get
		{
			if(_ufle12jhs77_F_yAcc==null || _ufle12jhs77_F_yAcc.IsDeleted) _ufle12jhs77_F_yAcc=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(4837300759352897222UL,224404043170575504UL));
			return _ufle12jhs77_F_yAcc;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_horiAcc;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_horiAcc
	{
		get
		{
			if(_ufle12jhs77_F_horiAcc==null || _ufle12jhs77_F_horiAcc.IsDeleted) _ufle12jhs77_F_horiAcc=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5071652817814942469UL,173174974071651755UL));
			return _ufle12jhs77_F_horiAcc;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_Acc;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_Acc
	{
		get
		{
			if(_ufle12jhs77_F_Acc==null || _ufle12jhs77_F_Acc.IsDeleted) _ufle12jhs77_F_Acc=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(4673709057513295624UL,4561040612170999975UL));
			return _ufle12jhs77_F_Acc;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_F_dropCount;
	public static BansheeGz.BGDatabase.BGFieldInt _F_dropCount
	{
		get
		{
			if(_ufle12jhs77_F_dropCount==null || _ufle12jhs77_F_dropCount.IsDeleted) _ufle12jhs77_F_dropCount=(BansheeGz.BGDatabase.BGFieldInt) MetaDefault.GetField(new BGId(4886694361553423016UL,18094290060984744093UL));
			return _ufle12jhs77_F_dropCount;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_F_escaped;
	public static BansheeGz.BGDatabase.BGFieldInt _F_escaped
	{
		get
		{
			if(_ufle12jhs77_F_escaped==null || _ufle12jhs77_F_escaped.IsDeleted) _ufle12jhs77_F_escaped=(BansheeGz.BGDatabase.BGFieldInt) MetaDefault.GetField(new BGId(5516643015721476985UL,15701446706182041989UL));
			return _ufle12jhs77_F_escaped;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_F_escapredPerSec;
	public static BansheeGz.BGDatabase.BGFieldInt _F_escapredPerSec
	{
		get
		{
			if(_ufle12jhs77_F_escapredPerSec==null || _ufle12jhs77_F_escapredPerSec.IsDeleted) _ufle12jhs77_F_escapredPerSec=(BansheeGz.BGDatabase.BGFieldInt) MetaDefault.GetField(new BGId(5688767924342704680UL,14832174436515415477UL));
			return _ufle12jhs77_F_escapredPerSec;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldString _ufle12jhs77_F_GroupID;
	public static BansheeGz.BGDatabase.BGFieldString _F_GroupID
	{
		get
		{
			if(_ufle12jhs77_F_GroupID==null || _ufle12jhs77_F_GroupID.IsDeleted) _ufle12jhs77_F_GroupID=(BansheeGz.BGDatabase.BGFieldString) MetaDefault.GetField(new BGId(5247665724986267048UL,9688379696356197511UL));
			return _ufle12jhs77_F_GroupID;
		}
	}
	private static readonly DB_Detail.Factory _factory0_PFS = new DB_Detail.Factory();
	private static readonly DB_Eq.Factory _factory1_PFS = new DB_Eq.Factory();
	private static readonly DB_Summary.Factory _factory2_PFS = new DB_Summary.Factory();
	private DB_Detail() : base(MetaDefault)
	{
	}
	private DB_Detail(BGId id) : base(MetaDefault, id)
	{
	}
	private DB_Detail(BGMetaEntity meta) : base(meta)
	{
	}
	private DB_Detail(BGMetaEntity meta, BGId id) : base(meta, id)
	{
	}
	public static DB_Detail FindEntity(Predicate<DB_Detail> filter)
	{
		return MetaDefault.FindEntity(entity => filter==null || filter((DB_Detail) entity)) as DB_Detail;
	}
	public static List<DB_Detail> FindEntities(Predicate<DB_Detail> filter, List<DB_Detail> result=null, Comparison<DB_Detail> sort=null)
	{
		result = result ?? new List<DB_Detail>();
		_find_Entities_Result.Clear();
		MetaDefault.FindEntities(filter == null ? (Predicate<BGEntity>) null: e => filter((DB_Detail) e), _find_Entities_Result, sort == null ? (Comparison<BGEntity>) null : (e1, e2) => sort((DB_Detail) e1, (DB_Detail) e2));
		if (_find_Entities_Result.Count != 0)
		{
			for (var i = 0; i < _find_Entities_Result.Count; i++) result.Add((DB_Detail) _find_Entities_Result[i]);
			_find_Entities_Result.Clear();
		}
		return result;
	}
	public static void ForEachEntity(Action<DB_Detail> action, Predicate<DB_Detail> filter=null, Comparison<DB_Detail> sort=null)
	{
		MetaDefault.ForEachEntity(entity => action((DB_Detail) entity), filter == null ? null : (Predicate<BGEntity>) (entity => filter((DB_Detail) entity)), sort==null?(Comparison<BGEntity>) null:(e1,e2) => sort((DB_Detail)e1,(DB_Detail)e2));
	}
	public static DB_Detail GetEntity(BGId entityId)
	{
		return (DB_Detail) MetaDefault.GetEntity(entityId);
	}
	public static DB_Detail GetEntity(int index)
	{
		return (DB_Detail) MetaDefault[index];
	}
	public static DB_Detail GetEntity(string entityName)
	{
		return (DB_Detail) MetaDefault.GetEntity(entityName);
	}
	public static DB_Detail NewEntity()
	{
		return (DB_Detail) MetaDefault.NewEntity();
	}
}

//=============================================================
//||                   Generated by BansheeGz Code Generator ||
//=============================================================

public partial class DB_Eq : BGEntity
{

	//=============================================================
	//||                   Generated by BansheeGz Code Generator ||
	//=============================================================

	public class Factory : BGEntity.EntityFactory
	{
		public BGEntity NewEntity(BGMetaEntity meta)
		{
			return new DB_Eq(meta);
		}
		public BGEntity NewEntity(BGMetaEntity meta, BGId id)
		{
			return new DB_Eq(meta, id);
		}
	}
	private static BansheeGz.BGDatabase.BGMetaRow _metaDefault;
	public static BansheeGz.BGDatabase.BGMetaRow MetaDefault
	{
		get
		{
			if(_metaDefault==null || _metaDefault.IsDeleted) _metaDefault=BGRepo.I.GetMeta<BansheeGz.BGDatabase.BGMetaRow>(new BGId(5466638798205139117UL,16259843907133185205UL));
			return _metaDefault;
		}
	}
	public static BansheeGz.BGDatabase.BGRepoEvents Events
	{
		get
		{
			return BGRepo.I.Events;
		}
	}
	private static readonly List<BGEntity> _find_Entities_Result = new List<BGEntity>();
	public static int CountEntities
	{
		get
		{
			return MetaDefault.CountEntities;
		}
	}
	public System.String F_name
	{
		get
		{
			return _F_name[Index];
		}
		set
		{
			_F_name[Index] = value;
		}
	}
	public System.Int32 F_eqIndex
	{
		get
		{
			return _F_eqIndex[Index];
		}
		set
		{
			_F_eqIndex[Index] = value;
		}
	}
	public System.String F_eqName
	{
		get
		{
			return _F_eqName[Index];
		}
		set
		{
			_F_eqName[Index] = value;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldEntityName _ufle12jhs77_F_name;
	public static BansheeGz.BGDatabase.BGFieldEntityName _F_name
	{
		get
		{
			if(_ufle12jhs77_F_name==null || _ufle12jhs77_F_name.IsDeleted) _ufle12jhs77_F_name=(BansheeGz.BGDatabase.BGFieldEntityName) MetaDefault.GetField(new BGId(5624363048074889713UL,4143900681313895326UL));
			return _ufle12jhs77_F_name;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_F_eqIndex;
	public static BansheeGz.BGDatabase.BGFieldInt _F_eqIndex
	{
		get
		{
			if(_ufle12jhs77_F_eqIndex==null || _ufle12jhs77_F_eqIndex.IsDeleted) _ufle12jhs77_F_eqIndex=(BansheeGz.BGDatabase.BGFieldInt) MetaDefault.GetField(new BGId(4942318543007949863UL,2075134111947045770UL));
			return _ufle12jhs77_F_eqIndex;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldString _ufle12jhs77_F_eqName;
	public static BansheeGz.BGDatabase.BGFieldString _F_eqName
	{
		get
		{
			if(_ufle12jhs77_F_eqName==null || _ufle12jhs77_F_eqName.IsDeleted) _ufle12jhs77_F_eqName=(BansheeGz.BGDatabase.BGFieldString) MetaDefault.GetField(new BGId(5196298831350822805UL,2741710381728140686UL));
			return _ufle12jhs77_F_eqName;
		}
	}
	private static readonly DB_Detail.Factory _factory0_PFS = new DB_Detail.Factory();
	private static readonly DB_Eq.Factory _factory1_PFS = new DB_Eq.Factory();
	private static readonly DB_Summary.Factory _factory2_PFS = new DB_Summary.Factory();
	private DB_Eq() : base(MetaDefault)
	{
	}
	private DB_Eq(BGId id) : base(MetaDefault, id)
	{
	}
	private DB_Eq(BGMetaEntity meta) : base(meta)
	{
	}
	private DB_Eq(BGMetaEntity meta, BGId id) : base(meta, id)
	{
	}
	public static DB_Eq FindEntity(Predicate<DB_Eq> filter)
	{
		return MetaDefault.FindEntity(entity => filter==null || filter((DB_Eq) entity)) as DB_Eq;
	}
	public static List<DB_Eq> FindEntities(Predicate<DB_Eq> filter, List<DB_Eq> result=null, Comparison<DB_Eq> sort=null)
	{
		result = result ?? new List<DB_Eq>();
		_find_Entities_Result.Clear();
		MetaDefault.FindEntities(filter == null ? (Predicate<BGEntity>) null: e => filter((DB_Eq) e), _find_Entities_Result, sort == null ? (Comparison<BGEntity>) null : (e1, e2) => sort((DB_Eq) e1, (DB_Eq) e2));
		if (_find_Entities_Result.Count != 0)
		{
			for (var i = 0; i < _find_Entities_Result.Count; i++) result.Add((DB_Eq) _find_Entities_Result[i]);
			_find_Entities_Result.Clear();
		}
		return result;
	}
	public static void ForEachEntity(Action<DB_Eq> action, Predicate<DB_Eq> filter=null, Comparison<DB_Eq> sort=null)
	{
		MetaDefault.ForEachEntity(entity => action((DB_Eq) entity), filter == null ? null : (Predicate<BGEntity>) (entity => filter((DB_Eq) entity)), sort==null?(Comparison<BGEntity>) null:(e1,e2) => sort((DB_Eq)e1,(DB_Eq)e2));
	}
	public static DB_Eq GetEntity(BGId entityId)
	{
		return (DB_Eq) MetaDefault.GetEntity(entityId);
	}
	public static DB_Eq GetEntity(int index)
	{
		return (DB_Eq) MetaDefault[index];
	}
	public static DB_Eq GetEntity(string entityName)
	{
		return (DB_Eq) MetaDefault.GetEntity(entityName);
	}
	public static DB_Eq NewEntity()
	{
		return (DB_Eq) MetaDefault.NewEntity();
	}
}

//=============================================================
//||                   Generated by BansheeGz Code Generator ||
//=============================================================

public partial class DB_Summary : BGEntity
{

	//=============================================================
	//||                   Generated by BansheeGz Code Generator ||
	//=============================================================

	public class Factory : BGEntity.EntityFactory
	{
		public BGEntity NewEntity(BGMetaEntity meta)
		{
			return new DB_Summary(meta);
		}
		public BGEntity NewEntity(BGMetaEntity meta, BGId id)
		{
			return new DB_Summary(meta, id);
		}
	}
	private static BansheeGz.BGDatabase.BGMetaRow _metaDefault;
	public static BansheeGz.BGDatabase.BGMetaRow MetaDefault
	{
		get
		{
			if(_metaDefault==null || _metaDefault.IsDeleted) _metaDefault=BGRepo.I.GetMeta<BansheeGz.BGDatabase.BGMetaRow>(new BGId(5655560454797276954UL,12225900667250882691UL));
			return _metaDefault;
		}
	}
	public static BansheeGz.BGDatabase.BGRepoEvents Events
	{
		get
		{
			return BGRepo.I.Events;
		}
	}
	private static readonly List<BGEntity> _find_Entities_Result = new List<BGEntity>();
	public static int CountEntities
	{
		get
		{
			return MetaDefault.CountEntities;
		}
	}
	public System.String F_name
	{
		get
		{
			return _F_name[Index];
		}
		set
		{
			_F_name[Index] = value;
		}
	}
	public System.Int32 F_eqIndex
	{
		get
		{
			return _F_eqIndex[Index];
		}
		set
		{
			_F_eqIndex[Index] = value;
		}
	}
	public System.Single F_PGA
	{
		get
		{
			return _F_PGA[Index];
		}
		set
		{
			_F_PGA[Index] = value;
		}
	}
	public System.Int32 F_finalDrop
	{
		get
		{
			return _F_finalDrop[Index];
		}
		set
		{
			_F_finalDrop[Index] = value;
		}
	}
	public System.Int32 F_itemCount
	{
		get
		{
			return _F_itemCount[Index];
		}
		set
		{
			_F_itemCount[Index] = value;
		}
	}
	public System.Single F_simulationTime
	{
		get
		{
			return _F_simulationTime[Index];
		}
		set
		{
			_F_simulationTime[Index] = value;
		}
	}
	public System.Single F_reactionTIme_ave
	{
		get
		{
			return _F_reactionTIme_ave[Index];
		}
		set
		{
			_F_reactionTIme_ave[Index] = value;
		}
	}
	public System.Single F_reactionTIme_min
	{
		get
		{
			return _F_reactionTIme_min[Index];
		}
		set
		{
			_F_reactionTIme_min[Index] = value;
		}
	}
	public System.Single F_reactionTIme_max
	{
		get
		{
			return _F_reactionTIme_max[Index];
		}
		set
		{
			_F_reactionTIme_max[Index] = value;
		}
	}
	public System.Single F_escapeTIme_ave
	{
		get
		{
			return _F_escapeTIme_ave[Index];
		}
		set
		{
			_F_escapeTIme_ave[Index] = value;
		}
	}
	public System.Single F_escapeTIme_min
	{
		get
		{
			return _F_escapeTIme_min[Index];
		}
		set
		{
			_F_escapeTIme_min[Index] = value;
		}
	}
	public System.Single F_escapeTIme_max
	{
		get
		{
			return _F_escapeTIme_max[Index];
		}
		set
		{
			_F_escapeTIme_max[Index] = value;
		}
	}
	public System.Single F_escapeLength_ave
	{
		get
		{
			return _F_escapeLength_ave[Index];
		}
		set
		{
			_F_escapeLength_ave[Index] = value;
		}
	}
	public System.Single F_escapeLength_min
	{
		get
		{
			return _F_escapeLength_min[Index];
		}
		set
		{
			_F_escapeLength_min[Index] = value;
		}
	}
	public System.Single F_escapeLength_max
	{
		get
		{
			return _F_escapeLength_max[Index];
		}
		set
		{
			_F_escapeLength_max[Index] = value;
		}
	}
	public System.Single F_vel_ave
	{
		get
		{
			return _F_vel_ave[Index];
		}
		set
		{
			_F_vel_ave[Index] = value;
		}
	}
	public System.Single F_vel_min
	{
		get
		{
			return _F_vel_min[Index];
		}
		set
		{
			_F_vel_min[Index] = value;
		}
	}
	public System.Single F_vel_max
	{
		get
		{
			return _F_vel_max[Index];
		}
		set
		{
			_F_vel_max[Index] = value;
		}
	}
	public System.String F_GroupID
	{
		get
		{
			return _F_GroupID[Index];
		}
		set
		{
			_F_GroupID[Index] = value;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldEntityName _ufle12jhs77_F_name;
	public static BansheeGz.BGDatabase.BGFieldEntityName _F_name
	{
		get
		{
			if(_ufle12jhs77_F_name==null || _ufle12jhs77_F_name.IsDeleted) _ufle12jhs77_F_name=(BansheeGz.BGDatabase.BGFieldEntityName) MetaDefault.GetField(new BGId(5505953492161453851UL,11104729378404957876UL));
			return _ufle12jhs77_F_name;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_F_eqIndex;
	public static BansheeGz.BGDatabase.BGFieldInt _F_eqIndex
	{
		get
		{
			if(_ufle12jhs77_F_eqIndex==null || _ufle12jhs77_F_eqIndex.IsDeleted) _ufle12jhs77_F_eqIndex=(BansheeGz.BGDatabase.BGFieldInt) MetaDefault.GetField(new BGId(5318355610638700245UL,13268480688012415380UL));
			return _ufle12jhs77_F_eqIndex;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_PGA;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_PGA
	{
		get
		{
			if(_ufle12jhs77_F_PGA==null || _ufle12jhs77_F_PGA.IsDeleted) _ufle12jhs77_F_PGA=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5315612655475886099UL,7227685176087190657UL));
			return _ufle12jhs77_F_PGA;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_F_finalDrop;
	public static BansheeGz.BGDatabase.BGFieldInt _F_finalDrop
	{
		get
		{
			if(_ufle12jhs77_F_finalDrop==null || _ufle12jhs77_F_finalDrop.IsDeleted) _ufle12jhs77_F_finalDrop=(BansheeGz.BGDatabase.BGFieldInt) MetaDefault.GetField(new BGId(4934815772182634162UL,9993293708270838156UL));
			return _ufle12jhs77_F_finalDrop;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_F_itemCount;
	public static BansheeGz.BGDatabase.BGFieldInt _F_itemCount
	{
		get
		{
			if(_ufle12jhs77_F_itemCount==null || _ufle12jhs77_F_itemCount.IsDeleted) _ufle12jhs77_F_itemCount=(BansheeGz.BGDatabase.BGFieldInt) MetaDefault.GetField(new BGId(5183886792353541873UL,65713430298064307UL));
			return _ufle12jhs77_F_itemCount;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_simulationTime;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_simulationTime
	{
		get
		{
			if(_ufle12jhs77_F_simulationTime==null || _ufle12jhs77_F_simulationTime.IsDeleted) _ufle12jhs77_F_simulationTime=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(4765886187894296917UL,4908088481250146198UL));
			return _ufle12jhs77_F_simulationTime;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_reactionTIme_ave;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_reactionTIme_ave
	{
		get
		{
			if(_ufle12jhs77_F_reactionTIme_ave==null || _ufle12jhs77_F_reactionTIme_ave.IsDeleted) _ufle12jhs77_F_reactionTIme_ave=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(4698146916101593241UL,2500934234736512398UL));
			return _ufle12jhs77_F_reactionTIme_ave;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_reactionTIme_min;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_reactionTIme_min
	{
		get
		{
			if(_ufle12jhs77_F_reactionTIme_min==null || _ufle12jhs77_F_reactionTIme_min.IsDeleted) _ufle12jhs77_F_reactionTIme_min=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5378768338366046738UL,7535247065446403996UL));
			return _ufle12jhs77_F_reactionTIme_min;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_reactionTIme_max;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_reactionTIme_max
	{
		get
		{
			if(_ufle12jhs77_F_reactionTIme_max==null || _ufle12jhs77_F_reactionTIme_max.IsDeleted) _ufle12jhs77_F_reactionTIme_max=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5694352428283209765UL,2681379727484566970UL));
			return _ufle12jhs77_F_reactionTIme_max;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_escapeTIme_ave;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_escapeTIme_ave
	{
		get
		{
			if(_ufle12jhs77_F_escapeTIme_ave==null || _ufle12jhs77_F_escapeTIme_ave.IsDeleted) _ufle12jhs77_F_escapeTIme_ave=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5212482698973865373UL,3816724647317693060UL));
			return _ufle12jhs77_F_escapeTIme_ave;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_escapeTIme_min;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_escapeTIme_min
	{
		get
		{
			if(_ufle12jhs77_F_escapeTIme_min==null || _ufle12jhs77_F_escapeTIme_min.IsDeleted) _ufle12jhs77_F_escapeTIme_min=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5352015652405569635UL,6846708334796447622UL));
			return _ufle12jhs77_F_escapeTIme_min;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_escapeTIme_max;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_escapeTIme_max
	{
		get
		{
			if(_ufle12jhs77_F_escapeTIme_max==null || _ufle12jhs77_F_escapeTIme_max.IsDeleted) _ufle12jhs77_F_escapeTIme_max=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(4614663973447401171UL,3759681356998053250UL));
			return _ufle12jhs77_F_escapeTIme_max;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_escapeLength_ave;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_escapeLength_ave
	{
		get
		{
			if(_ufle12jhs77_F_escapeLength_ave==null || _ufle12jhs77_F_escapeLength_ave.IsDeleted) _ufle12jhs77_F_escapeLength_ave=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(4835598022678539227UL,13054947179287942330UL));
			return _ufle12jhs77_F_escapeLength_ave;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_escapeLength_min;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_escapeLength_min
	{
		get
		{
			if(_ufle12jhs77_F_escapeLength_min==null || _ufle12jhs77_F_escapeLength_min.IsDeleted) _ufle12jhs77_F_escapeLength_min=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(4690682699160406393UL,12482032135857797273UL));
			return _ufle12jhs77_F_escapeLength_min;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_escapeLength_max;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_escapeLength_max
	{
		get
		{
			if(_ufle12jhs77_F_escapeLength_max==null || _ufle12jhs77_F_escapeLength_max.IsDeleted) _ufle12jhs77_F_escapeLength_max=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5288883079770257348UL,13555099439447561861UL));
			return _ufle12jhs77_F_escapeLength_max;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_vel_ave;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_vel_ave
	{
		get
		{
			if(_ufle12jhs77_F_vel_ave==null || _ufle12jhs77_F_vel_ave.IsDeleted) _ufle12jhs77_F_vel_ave=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5455327766858307826UL,14564464141471851156UL));
			return _ufle12jhs77_F_vel_ave;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_vel_min;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_vel_min
	{
		get
		{
			if(_ufle12jhs77_F_vel_min==null || _ufle12jhs77_F_vel_min.IsDeleted) _ufle12jhs77_F_vel_min=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5399967649891925385UL,879362064704177055UL));
			return _ufle12jhs77_F_vel_min;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_F_vel_max;
	public static BansheeGz.BGDatabase.BGFieldFloat _F_vel_max
	{
		get
		{
			if(_ufle12jhs77_F_vel_max==null || _ufle12jhs77_F_vel_max.IsDeleted) _ufle12jhs77_F_vel_max=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(4920144616963575440UL,16706110664453375923UL));
			return _ufle12jhs77_F_vel_max;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldString _ufle12jhs77_F_GroupID;
	public static BansheeGz.BGDatabase.BGFieldString _F_GroupID
	{
		get
		{
			if(_ufle12jhs77_F_GroupID==null || _ufle12jhs77_F_GroupID.IsDeleted) _ufle12jhs77_F_GroupID=(BansheeGz.BGDatabase.BGFieldString) MetaDefault.GetField(new BGId(5680798934586090079UL,11814331542341058441UL));
			return _ufle12jhs77_F_GroupID;
		}
	}
	private static readonly DB_Detail.Factory _factory0_PFS = new DB_Detail.Factory();
	private static readonly DB_Eq.Factory _factory1_PFS = new DB_Eq.Factory();
	private static readonly DB_Summary.Factory _factory2_PFS = new DB_Summary.Factory();
	private DB_Summary() : base(MetaDefault)
	{
	}
	private DB_Summary(BGId id) : base(MetaDefault, id)
	{
	}
	private DB_Summary(BGMetaEntity meta) : base(meta)
	{
	}
	private DB_Summary(BGMetaEntity meta, BGId id) : base(meta, id)
	{
	}
	public static DB_Summary FindEntity(Predicate<DB_Summary> filter)
	{
		return MetaDefault.FindEntity(entity => filter==null || filter((DB_Summary) entity)) as DB_Summary;
	}
	public static List<DB_Summary> FindEntities(Predicate<DB_Summary> filter, List<DB_Summary> result=null, Comparison<DB_Summary> sort=null)
	{
		result = result ?? new List<DB_Summary>();
		_find_Entities_Result.Clear();
		MetaDefault.FindEntities(filter == null ? (Predicate<BGEntity>) null: e => filter((DB_Summary) e), _find_Entities_Result, sort == null ? (Comparison<BGEntity>) null : (e1, e2) => sort((DB_Summary) e1, (DB_Summary) e2));
		if (_find_Entities_Result.Count != 0)
		{
			for (var i = 0; i < _find_Entities_Result.Count; i++) result.Add((DB_Summary) _find_Entities_Result[i]);
			_find_Entities_Result.Clear();
		}
		return result;
	}
	public static void ForEachEntity(Action<DB_Summary> action, Predicate<DB_Summary> filter=null, Comparison<DB_Summary> sort=null)
	{
		MetaDefault.ForEachEntity(entity => action((DB_Summary) entity), filter == null ? null : (Predicate<BGEntity>) (entity => filter((DB_Summary) entity)), sort==null?(Comparison<BGEntity>) null:(e1,e2) => sort((DB_Summary)e1,(DB_Summary)e2));
	}
	public static DB_Summary GetEntity(BGId entityId)
	{
		return (DB_Summary) MetaDefault.GetEntity(entityId);
	}
	public static DB_Summary GetEntity(int index)
	{
		return (DB_Summary) MetaDefault[index];
	}
	public static DB_Summary GetEntity(string entityName)
	{
		return (DB_Summary) MetaDefault.GetEntity(entityName);
	}
	public static DB_Summary NewEntity()
	{
		return (DB_Summary) MetaDefault.NewEntity();
	}
}
#pragma warning restore 414
