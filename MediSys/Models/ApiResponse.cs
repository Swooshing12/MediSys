using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediSys.Models
{
	public class ApiResponse<T>
	{
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public T? Data { get; set; }
		public string Timestamp { get; set; } = string.Empty;
		public int Code { get; set; }
		public object? Details { get; set; }
	}

	public class ApiResponse : ApiResponse<object>
	{
	}
}