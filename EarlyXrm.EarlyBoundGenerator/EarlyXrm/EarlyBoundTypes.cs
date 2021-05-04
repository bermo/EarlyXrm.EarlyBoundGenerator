//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute()]

namespace EarlyBoundTypes
{
	using System;
	using System.Linq;
	using Microsoft.Xrm.Sdk;
	using System.Runtime.Serialization;
	using Microsoft.Xrm.Sdk.Client;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	
	
	[DataContract()]
	public enum ComponentType
	{
		
		[Description("AI Configuration")]
		[EnumMember()]
		AIConfiguration = 402,
		
		[Description("AI Project")]
		[EnumMember()]
		AIProject = 401,
		
		[Description("AI Project Type")]
		[EnumMember()]
		AIProjectType = 400,
		
		[Description("Attachment")]
		[EnumMember()]
		Attachment = 35,
		
		[Description("Attribute")]
		[EnumMember()]
		Attribute = 2,
		
		[Description("Attribute Image Configuration")]
		[EnumMember()]
		AttributeImageConfiguration = 431,
		
		[Description("Attribute Lookup Value")]
		[EnumMember()]
		AttributeLookupValue = 5,
		
		[Description("Attribute Map")]
		[EnumMember()]
		AttributeMap = 47,
		
		[Description("Attribute Picklist Value")]
		[EnumMember()]
		AttributePicklistValue = 4,
		
		[Description("Canvas App")]
		[EnumMember()]
		CanvasApp = 300,
		
		[Description("Complex Control")]
		[EnumMember()]
		ComplexControl = 64,
		
		[Description("Connection Role")]
		[EnumMember()]
		ConnectionRole = 63,
		
		[Description("Connector")]
		[EnumMember()]
		Connector = 371,
		
		[Description("Connector")]
		[EnumMember()]
		Connector2 = 372,
		
		[Description("Contract Template")]
		[EnumMember()]
		ContractTemplate = 37,
		
		[Description("Convert Rule")]
		[EnumMember()]
		ConvertRule = 154,
		
		[Description("Convert Rule Item")]
		[EnumMember()]
		ConvertRuleItem = 155,
		
		[Description("Custom Control")]
		[EnumMember()]
		CustomControl = 66,
		
		[Description("Custom Control Default Config")]
		[EnumMember()]
		CustomControlDefaultConfig = 68,
		
		[Description("Data Source Mapping")]
		[EnumMember()]
		DataSourceMapping = 166,
		
		[Description("Display String")]
		[EnumMember()]
		DisplayString = 22,
		
		[Description("Display String Map")]
		[EnumMember()]
		DisplayStringMap = 23,
		
		[Description("Duplicate Rule")]
		[EnumMember()]
		DuplicateRule = 44,
		
		[Description("Duplicate Rule Condition")]
		[EnumMember()]
		DuplicateRuleCondition = 45,
		
		[Description("Email Template")]
		[EnumMember()]
		EmailTemplate = 36,
		
		[Description("Entity")]
		[EnumMember()]
		Entity = 1,
		
		[Description("Entity Analytics Configuration")]
		[EnumMember()]
		EntityAnalyticsConfiguration = 430,
		
		[Description("Entity Image Configuration")]
		[EnumMember()]
		EntityImageConfiguration = 432,
		
		[Description("Entity Key")]
		[EnumMember()]
		EntityKey = 14,
		
		[Description("Entity Map")]
		[EnumMember()]
		EntityMap = 46,
		
		[Description("Entity Relationship")]
		[EnumMember()]
		EntityRelationship = 10,
		
		[Description("Entity Relationship Relationships")]
		[EnumMember()]
		EntityRelationshipRelationships = 12,
		
		[Description("Entity Relationship Role")]
		[EnumMember()]
		EntityRelationshipRole = 11,
		
		[Description("Environment Variable Definition")]
		[EnumMember()]
		EnvironmentVariableDefinition = 380,
		
		[Description("Environment Variable Value")]
		[EnumMember()]
		EnvironmentVariableValue = 381,
		
		[Description("Field Permission")]
		[EnumMember()]
		FieldPermission = 71,
		
		[Description("Field Security Profile")]
		[EnumMember()]
		FieldSecurityProfile = 70,
		
		[Description("Form")]
		[EnumMember()]
		Form = 24,
		
		[Description("Hierarchy Rule")]
		[EnumMember()]
		HierarchyRule = 65,
		
		[Description("Import Map")]
		[EnumMember()]
		ImportMap = 208,
		
		[Description("Index")]
		[EnumMember()]
		Index = 18,
		
		[Description("KB Article Template")]
		[EnumMember()]
		KBArticleTemplate = 38,
		
		[Description("Localized Label")]
		[EnumMember()]
		LocalizedLabel = 7,
		
		[Description("Mail Merge Template")]
		[EnumMember()]
		MailMergeTemplate = 39,
		
		[Description("Managed Property")]
		[EnumMember()]
		ManagedProperty = 13,
		
		[Description("Mobile Offline Profile")]
		[EnumMember()]
		MobileOfflineProfile = 161,
		
		[Description("Mobile Offline Profile Item")]
		[EnumMember()]
		MobileOfflineProfileItem = 162,
		
		[Description("Option Set")]
		[EnumMember()]
		OptionSet = 9,
		
		[Description("Organization")]
		[EnumMember()]
		Organization = 25,
		
		[Description("Plugin Assembly")]
		[EnumMember()]
		PluginAssembly = 91,
		
		[Description("Plugin Type")]
		[EnumMember()]
		PluginType = 90,
		
		[Description("Privilege")]
		[EnumMember()]
		Privilege = 16,
		
		[Description("PrivilegeObjectTypeCode")]
		[EnumMember()]
		PrivilegeObjectTypeCode = 17,
		
		[Description("Relationship")]
		[EnumMember()]
		Relationship = 3,
		
		[Description("Relationship Extra Condition")]
		[EnumMember()]
		RelationshipExtraCondition = 8,
		
		[Description("Report")]
		[EnumMember()]
		Report = 31,
		
		[Description("Report Category")]
		[EnumMember()]
		ReportCategory = 33,
		
		[Description("Report Entity")]
		[EnumMember()]
		ReportEntity = 32,
		
		[Description("Report Visibility")]
		[EnumMember()]
		ReportVisibility = 34,
		
		[Description("Ribbon Command")]
		[EnumMember()]
		RibbonCommand = 48,
		
		[Description("Ribbon Context Group")]
		[EnumMember()]
		RibbonContextGroup = 49,
		
		[Description("Ribbon Customization")]
		[EnumMember()]
		RibbonCustomization = 50,
		
		[Description("Ribbon Diff")]
		[EnumMember()]
		RibbonDiff = 55,
		
		[Description("Ribbon Rule")]
		[EnumMember()]
		RibbonRule = 52,
		
		[Description("Ribbon Tab To Command Map")]
		[EnumMember()]
		RibbonTabToCommandMap = 53,
		
		[Description("Role")]
		[EnumMember()]
		Role = 20,
		
		[Description("Role Privilege")]
		[EnumMember()]
		RolePrivilege = 21,
		
		[Description("Routing Rule")]
		[EnumMember()]
		RoutingRule = 150,
		
		[Description("Routing Rule Item")]
		[EnumMember()]
		RoutingRuleItem = 151,
		
		[Description("Saved Query")]
		[EnumMember()]
		SavedQuery = 26,
		
		[Description("Saved Query Visualization")]
		[EnumMember()]
		SavedQueryVisualization = 59,
		
		[Description("SDKMessage")]
		[EnumMember()]
		SDKMessage = 201,
		
		[Description("SDKMessageFilter")]
		[EnumMember()]
		SDKMessageFilter = 202,
		
		[Description("SdkMessagePair")]
		[EnumMember()]
		SdkMessagePair = 203,
		
		[Description("SDK Message Processing Step")]
		[EnumMember()]
		SDKMessageProcessingStep = 92,
		
		[Description("SDK Message Processing Step Image")]
		[EnumMember()]
		SDKMessageProcessingStepImage = 93,
		
		[Description("SdkMessageRequest")]
		[EnumMember()]
		SdkMessageRequest = 204,
		
		[Description("SdkMessageRequestField")]
		[EnumMember()]
		SdkMessageRequestField = 205,
		
		[Description("SdkMessageResponse")]
		[EnumMember()]
		SdkMessageResponse = 206,
		
		[Description("SdkMessageResponseField")]
		[EnumMember()]
		SdkMessageResponseField = 207,
		
		[Description("Service Endpoint")]
		[EnumMember()]
		ServiceEndpoint = 95,
		
		[Description("Similarity Rule")]
		[EnumMember()]
		SimilarityRule = 165,
		
		[Description("Site Map")]
		[EnumMember()]
		SiteMap = 62,
		
		[Description("SLA")]
		[EnumMember()]
		SLA = 152,
		
		[Description("SLA Item")]
		[EnumMember()]
		SLAItem = 153,
		
		[Description("System Form")]
		[EnumMember()]
		SystemForm = 60,
		
		[Description("View Attribute")]
		[EnumMember()]
		ViewAttribute = 6,
		
		[Description("Web Resource")]
		[EnumMember()]
		WebResource = 61,
		
		[Description("WebWizard")]
		[EnumMember()]
		WebWizard = 210,
		
		[Description("Workflow")]
		[EnumMember()]
		Workflow = 29,
	}
	
	[DataContract()]
	[EntityLogicalNameAttribute("solution")]
	[ExcludeFromCodeCoverage()]
	public partial class Solution : EarlyEntity
	{
		
		public Solution() : 
				base(EntityLogicalName)
		{
		}
		
		[AttributeLogicalNameAttribute("description")]
		public string Description
		{
			get
			{
				return GetAttributeValue<string>("description");
			}
			set
			{
				SetAttributeValue("description", nameof(Description), value);
			}
		}
		
		[AttributeLogicalNameAttribute("friendlyname")]
		public string DisplayName
		{
			get
			{
				return GetAttributeValue<string>("friendlyname");
			}
			set
			{
				SetAttributeValue("friendlyname", nameof(DisplayName), value);
			}
		}
		
		public const string EntityLogicalCollectionName = "solutions";
		
		public const string EntityLogicalName = "solution";
		
		public const string EntitySetName = "solutions";
		
		[AttributeLogicalNameAttribute("solutionid")]
		public new virtual Guid Id
		{
			get
			{
				return base.Id != default ? base.Id : GetAttributeValue<Guid>("solutionid");
			}
			set
			{
				SetAttributeValue("solutionid", nameof(Id), value);
				base.Id = value;
			}
		}
		
		[AttributeLogicalNameAttribute("installedon")]
		public DateTime? InstalledOn
		{
			get
			{
				return GetAttributeValue<DateTime?>("installedon");
			}
			set
			{
				SetAttributeValue("installedon", nameof(InstalledOn), value);
			}
		}
		
		[AttributeLogicalNameAttribute("isvisible")]
		public bool? IsVisibleOutsidePlatform
		{
			get
			{
				return GetAttributeValue<bool?>("isvisible");
			}
			set
			{
				SetAttributeValue("isvisible", nameof(IsVisibleOutsidePlatform), value);
			}
		}
		
		[AttributeLogicalNameAttribute("uniquename")]
		public string Name
		{
			get
			{
				return GetAttributeValue<string>("uniquename");
			}
			set
			{
				SetAttributeValue("uniquename", nameof(Name), value);
			}
		}
		
		[AttributeLogicalNameAttribute("organizationid")]
		public EntityReference OrganizationRef
		{
			get
			{
				return GetAttributeValue<EntityReference>("organizationid");
			}
			set
			{
				SetAttributeValue("organizationid", nameof(OrganizationRef), value);
			}
		}
		
		[AttributeLogicalNameAttribute("ismanaged")]
		public bool? PackageType
		{
			get
			{
				return GetAttributeValue<bool?>("ismanaged");
			}
			set
			{
				SetAttributeValue("ismanaged", nameof(PackageType), value);
			}
		}
		
		[AttributeLogicalNameAttribute("parentsolutionid")]
		[RelationshipSchemaNameAttribute("solution_parent_solution", EntityRole.Referencing)]
		public Solution ParentSolution
		{
			get
			{
				return GetRelatedEntity<Solution>("solution_parent_solution", EntityRole.Referencing);
			}
		}
		
		[AttributeLogicalNameAttribute("parentsolutionid")]
		public EntityReference ParentSolutionRef
		{
			get
			{
				return GetAttributeValue<EntityReference>("parentsolutionid");
			}
			set
			{
				SetAttributeValue("parentsolutionid", nameof(ParentSolutionRef), value);
			}
		}
		
		[RelationshipSchemaNameAttribute("solution_parent_solution", EntityRole.Referenced)]
		public IEnumerable<Solution> ParentSolutionSolutions
		{
			get
			{
				return GetRelatedEntities<Solution>("solution_parent_solution", EntityRole.Referenced);
			}
			set
			{
				SetRelatedEntities<Solution>("solution_parent_solution", nameof(ParentSolutionSolutions), value, EntityRole.Referenced);
			}
		}
		
		[AttributeLogicalNameAttribute("pinpointsolutiondefaultlocale")]
		public string PinpointSolutionDefaultLocale
		{
			get
			{
				return GetAttributeValue<string>("pinpointsolutiondefaultlocale");
			}
			set
			{
				SetAttributeValue("pinpointsolutiondefaultlocale", nameof(PinpointSolutionDefaultLocale), value);
			}
		}
		
		[AttributeLogicalNameAttribute("publisherid")]
		public EntityReference PublisherRef
		{
			get
			{
				return GetAttributeValue<EntityReference>("publisherid");
			}
			set
			{
				SetAttributeValue("publisherid", nameof(PublisherRef), value);
			}
		}
		
		[RelationshipSchemaNameAttribute("solution_solutioncomponent")]
		public IEnumerable<SolutionComponent> SolutionComponents
		{
			get
			{
				return GetRelatedEntities<SolutionComponent>("solution_solutioncomponent");
			}
			set
			{
				SetRelatedEntities<SolutionComponent>("solution_solutioncomponent", nameof(SolutionComponents), value);
			}
		}
		
		[AttributeLogicalNameAttribute("solutionpackageversion")]
		public string SolutionPackageVersion
		{
			get
			{
				return GetAttributeValue<string>("solutionpackageversion");
			}
			set
			{
				SetAttributeValue("solutionpackageversion", nameof(SolutionPackageVersion), value);
			}
		}
		
		[AttributeLogicalNameAttribute("solutiontype")]
		public Enums.SolutionType? SolutionType
		{
			get
			{
				return (Enums.SolutionType?)GetAttributeValue<OptionSetValue>("solutiontype")?.Value;
			}
			set
			{
				SetAttributeValue("solutiontype", nameof(SolutionType), value.HasValue ? new OptionSetValue((int)value.Value) : null);
			}
		}
		
		[AttributeLogicalNameAttribute("updatedon")]
		public DateTime? UpdatedOn
		{
			get
			{
				return GetAttributeValue<DateTime?>("updatedon");
			}
			set
			{
				SetAttributeValue("updatedon", nameof(UpdatedOn), value);
			}
		}
		
		[AttributeLogicalNameAttribute("version")]
		public string Version
		{
			get
			{
				return GetAttributeValue<string>("version");
			}
			set
			{
				SetAttributeValue("version", nameof(Version), value);
			}
		}
		
		[AttributeLogicalNameAttribute("versionnumber")]
		public long? VersionNumber
		{
			get
			{
				return GetAttributeValue<long?>("versionnumber");
			}
			set
			{
				SetAttributeValue("versionnumber", nameof(VersionNumber), value);
			}
		}
		
		[DataContract()]
		public struct Enums
		{
			
			[DataContract()]
			public enum SolutionType
			{
				
				[Description("Internal")]
				[EnumMember()]
				Internal = 2,
				
				[Description("None")]
				[EnumMember()]
				None = 0,
				
				[Description("Snapshot")]
				[EnumMember()]
				Snapshot = 1,
			}
		}
		
		[DataContract()]
		public struct LogicalNames
		{
			
			public const string Description = "description";
			
			public const string DisplayName = "friendlyname";
			
			public const string Id = "solutionid";
			
			public const string InstalledOn = "installedon";
			
			public const string IsVisibleOutsidePlatform = "isvisible";
			
			public const string Name = "uniquename";
			
			public const string OrganizationRef = "organizationid";
			
			public const string PackageType = "ismanaged";
			
			public const string ParentSolutionRef = "parentsolutionid";
			
			public const string PinpointSolutionDefaultLocale = "pinpointsolutiondefaultlocale";
			
			public const string PublisherRef = "publisherid";
			
			public const string SolutionPackageVersion = "solutionpackageversion";
			
			public const string SolutionType = "solutiontype";
			
			public const string UpdatedOn = "updatedon";
			
			public const string Version = "version";
			
			public const string VersionNumber = "versionnumber";
		}
		
		[DataContract()]
		public struct Relationships
		{
			
			public const string ParentSolution = "solution_parent_solution";
			
			public const string ParentSolutionSolutions = "solution_parent_solution";
			
			public const string SolutionComponents = "solution_solutioncomponent";
		}
	}
	
	[DataContract()]
	[EntityLogicalNameAttribute("solutioncomponent")]
	[ExcludeFromCodeCoverage()]
	public partial class SolutionComponent : EarlyEntity
	{
		
		public SolutionComponent() : 
				base(EntityLogicalName)
		{
		}
		
		public const string EntityLogicalCollectionName = "solutioncomponentss";
		
		public const string EntityLogicalName = "solutioncomponent";
		
		public const string EntitySetName = "solutioncomponents";
		
		[AttributeLogicalNameAttribute("solutioncomponentid")]
		public new virtual Guid Id
		{
			get
			{
				return base.Id != default ? base.Id : GetAttributeValue<Guid>("solutioncomponentid");
			}
			set
			{
				SetAttributeValue("solutioncomponentid", nameof(Id), value);
				base.Id = value;
			}
		}
		
		[AttributeLogicalNameAttribute("objectid")]
		public Guid? ObjectId
		{
			get
			{
				return GetAttributeValue<Guid?>("objectid");
			}
			set
			{
				SetAttributeValue("objectid", nameof(ObjectId), value);
			}
		}
		
		[AttributeLogicalNameAttribute("componenttype")]
		public ComponentType? ObjectTypeCode
		{
			get
			{
				return (ComponentType?)GetAttributeValue<OptionSetValue>("componenttype")?.Value;
			}
			set
			{
				SetAttributeValue("componenttype", nameof(ObjectTypeCode), value.HasValue ? new OptionSetValue((int)value.Value) : null);
			}
		}
		
		[AttributeLogicalNameAttribute("rootcomponentbehavior")]
		public Enums.IncludeBehavior? RootComponentBehavior
		{
			get
			{
				return (Enums.IncludeBehavior?)GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value;
			}
			set
			{
				SetAttributeValue("rootcomponentbehavior", nameof(RootComponentBehavior), value.HasValue ? new OptionSetValue((int)value.Value) : null);
			}
		}
		
		[AttributeLogicalNameAttribute("rootsolutioncomponentid")]
		public Guid? RootSolutionComponentId
		{
			get
			{
				return GetAttributeValue<Guid?>("rootsolutioncomponentid");
			}
			set
			{
				SetAttributeValue("rootsolutioncomponentid", nameof(RootSolutionComponentId), value);
			}
		}
		
		[AttributeLogicalNameAttribute("solutionid")]
		[RelationshipSchemaNameAttribute("solution_solutioncomponent")]
		public Solution Solution1
		{
			get
			{
				return GetRelatedEntity<Solution>("solution_solutioncomponent");
			}
		}
		
		[AttributeLogicalNameAttribute("solutionid")]
		public EntityReference SolutionRef
		{
			get
			{
				return GetAttributeValue<EntityReference>("solutionid");
			}
			set
			{
				SetAttributeValue("solutionid", nameof(SolutionRef), value);
			}
		}
		
		[DataContract()]
		public struct Enums
		{
			
			[DataContract()]
			public enum IncludeBehavior
			{
				
				[Description("Do not include subcomponents")]
				[EnumMember()]
				DoNotIncludeSubcomponents = 1,
				
				[Description("Include As Shell Only")]
				[EnumMember()]
				IncludeAsShellOnly = 2,
				
				[Description("Include Subcomponents")]
				[EnumMember()]
				IncludeSubcomponents = 0,
			}
		}
		
		[DataContract()]
		public struct LogicalNames
		{
			
			public const string Id = "solutioncomponentid";
			
			public const string ObjectId = "objectid";
			
			public const string ObjectTypeCode = "componenttype";
			
			public const string RootComponentBehavior = "rootcomponentbehavior";
			
			public const string RootSolutionComponentId = "rootsolutioncomponentid";
			
			public const string SolutionRef = "solutionid";
		}
		
		[DataContract()]
		public struct Relationships
		{
			
			public const string Solution1 = "solution_solutioncomponent";
		}
	}
	
	[DataContract()]
	[ExcludeFromCodeCoverage()]
	public abstract class EarlyEntity : Entity, INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		public EarlyEntity(string entityLogicalName) : base(entityLogicalName) { }
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		protected void OnPropertyChanged(string propertyName)
        {
            if ((PropertyChanged != null))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
		
		protected void OnPropertyChanging(string propertyName)
        {
            if ((PropertyChanging != null))
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }
		
		public IEnumerable<T> GetAttributeValues<T>(string attributeLogicalName) where T : Entity
	    {
		    return base.GetAttributeValue<EntityCollection>(attributeLogicalName)?.Entities?.Cast<T>();
	    }
		
		protected void SetAttributeValues<T>(string logicalName, string attributePropertyName, IEnumerable<T> value)  where T : Entity
        {
            SetAttributeValue(logicalName, attributePropertyName, new EntityCollection(new List<Entity>(value)));
        }
		
		protected void SetAttributeValue(string logicalName, string attributePropertyName, object value)
        {
            OnPropertyChanging(attributePropertyName);
            base.SetAttributeValue(logicalName, value);
            OnPropertyChanged(attributePropertyName);
        }
		
		protected new T GetRelatedEntity<T>(string relationshipSchemaName, EntityRole? primaryEntityRole = null) where T : Entity
        {
            return base.GetRelatedEntity<T>(relationshipSchemaName, primaryEntityRole);
        }
		
		protected void SetRelatedEntity<T>(string relationshipSchemaName, string attributePropertyName, T entity, EntityRole? primaryEntityRole = null) where T : Entity
        {
            OnPropertyChanging(attributePropertyName);
            base.SetRelatedEntity(relationshipSchemaName, primaryEntityRole, entity);
            OnPropertyChanged(attributePropertyName);
        }
		
		protected new IEnumerable<T> GetRelatedEntities<T>(string relationshipSchemaName, EntityRole? primaryEntityRole = null) where T : Entity
        {
            return base.GetRelatedEntities<T>(relationshipSchemaName, primaryEntityRole);
        }
		
		protected void SetRelatedEntities<T>(string relationshipSchemaName, string attributePropertyName, IEnumerable<T> entities, EntityRole? primaryEntityRole = null) where T : Entity
        {
            OnPropertyChanging(attributePropertyName);
            base.SetRelatedEntities(relationshipSchemaName, primaryEntityRole, entities);
            OnPropertyChanged(attributePropertyName);
        }
	}
}
