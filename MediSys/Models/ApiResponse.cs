using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace MediSys.Models
{
	public class ApiResponse<T>
	{
		[JsonPropertyName("success")]
		public bool Success { get; set; }

		[JsonPropertyName("message")]
		public string Message { get; set; } = string.Empty;

		[JsonPropertyName("data")]
		public T? Data { get; set; }

		[JsonPropertyName("timestamp")]
		public string Timestamp { get; set; } = string.Empty;

		[JsonPropertyName("code")]
		public int Code { get; set; }

		[JsonPropertyName("details")]
		public object? Details { get; set; }
	}

	public class ApiResponse : ApiResponse<object>
	{
	}
}