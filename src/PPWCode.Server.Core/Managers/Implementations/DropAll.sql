if object_id(N'dbo.DropTables', N'P') is not null
  DROP PROCEDURE dbo.DropTables;
GO

create procedure dbo.DropTables
as
begin
  declare
    @err int,
    @cr cursor,
    @constraint_name sysname,
    @object_name sysname,
    @stmt nvarchar(1024),
    @i int,
    @recursive_fks nvarchar(4000);

  -- Drop foreign key constraints first.
  set @cr = cursor fast_forward for
    select fk.[name] as constraint_name,
           object_name(fk.parent_object_id) as table_name
      from sys.foreign_keys fk;
  open @cr;
  set @err = @@error;
  while(@err = 0)
  begin
    fetch next from @cr into
      @constraint_name,
      @object_name;
    set @err = @@error;
    if (@err <> 0) or (@@FETCH_STATUS <> 0)
      break;
    set @stmt = 'alter table dbo.['+@object_name+'] drop constraint '+@constraint_name;
    execute @err = sp_executesql @stmt;
  end
  close @cr;
  deallocate @cr;

  -- Drop tables
  set @cr = cursor fast_forward for
    with fk as (
      select fk.parent_object_id,
             object_name(fk.parent_object_id) as table_name,
             fk.referenced_object_id,
             object_name(fk.referenced_object_id) as referenced_table_name
        from sys.foreign_keys fk
       where fk.parent_object_id <> fk.referenced_object_id)

      select s.[name]+'.['+object_name(o.[object_id])+']' as [object_name]
        from sys.objects o
             inner join sys.schemas s on
               s.[schema_id] = o.[schema_id]
       where o.[type] = 'u'
         and s.[name] = 'dbo'
         and not exists (
               select null
                 from fk
                where fk.referenced_object_id = o.[object_id]);

  set @i = 0;
  while (@i < 20)
  begin
    open @cr;
    set @err = @@error;
    while(@err = 0)
    begin
      fetch next from @cr into
        @object_name;
      set @err = @@error;
      if (@err <> 0) or (@@FETCH_STATUS <> 0)
        break;
      set @stmt = 'drop table '+@object_name;
      execute @err = sp_executesql @stmt;
    end
    close @cr ;
    set @i = @i + 1;
  end
  deallocate @cr;
end;
GO

EXEC	[dbo].[DropTables];
go

if object_id(N'dbo.DropTables', N'P') is not null
  DROP PROCEDURE dbo.DropTables;
GO
