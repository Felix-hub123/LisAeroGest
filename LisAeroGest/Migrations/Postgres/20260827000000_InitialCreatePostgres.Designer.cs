using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using LisAeroGest.Data;

#nullable disable

namespace LisAeroGest.Migrations.Postgres
{
    [DbContext(typeof(DataContextPostgres))]
    [Migration("20260827000000_InitialCreatePostgres")]
    partial class InitialCreatePostgres
    {
    }
}