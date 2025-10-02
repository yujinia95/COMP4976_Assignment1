using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ObituaryApp.Mvc.Models;

public class Obituary
{
    [Key]
    [BindNever]
    public int Id { get; set; }

    public required string FullName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateOnly? DateOfDeath { get; set; }
    public string? Biography { get; set; }
    public byte[]? Photo { get; set; }
    public string? PhotoContentType { get; set; }
}
