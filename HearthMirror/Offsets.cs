namespace HearthMirror
{
	internal static class Offsets
	{
		public static uint ImageDosHeader_e_lfanew = 0x4d;
		public static uint ImageNTHeaders_Signature = 0x11;
		public static uint ImageNTHeaders_Machine = 0x15;
		public static uint ImageNTHeaders_ExportDirectoryAddress = 0x89;
		public static uint ImageExportDirectory_NumberOfFunctions = 0x25;
		public static uint ImageExportDirectory_AddressOfFunctions = 0x2d;
		public static uint ImageExportDirectory_AddressOfNames = 0x31;
		public static uint MonoDomain_sizeof = 0x155;
		public static uint MonoDomain_domain_assemblies = 0x81;
		public static uint MonoAssembly_sizeof = 0x65;
		public static uint MonoAssembly_name = 0x19;
		public static uint MonoAssembly_image = 0x51;
		public static uint MonoImage_class_cache = 0x2b1;
		public static uint MonoInternalHashTable_size = 0x1d;
		public static uint MonoInternalHashTable_table = 0x25;
		public static uint MonoClass_parent = 0x35;
		public static uint MonoClass_nested_in = 0x39;
		public static uint MonoClass_runtime_info = 0xb5;
		public static uint MonoClass_name = 0x41;
		public static uint MonoClass_name_space = 0x45;
		public static uint MonoClass_next_class_cache = 0xb9;
		public static uint MonoClass_fields = 0x85;
		public static uint MonoClass_sizes = 0x69;
		public static uint MonoClass_byval_arg = 0x99;
		public static uint MonoClass_bitfields = 0x25;
		public static uint MonoClass_field_count = 0x75;
		public static uint MonoClassField_sizeof = 0x21;
		public static uint MonoClassField_type = 0x11;
		public static uint MonoClassField_name = 0x15;
		public static uint MonoClassField_parent = 0x19;
		public static uint MonoClassField_offset = 0x1d;
		public static uint MonoType_attrs = 0x15;
		public static uint MonoType_sizeof = 0x19;
		public static uint MonoClassRuntimeInfo_domain_vtables = 0x15;
		public static uint MonoVTable_data = 0x1d;
	}
}