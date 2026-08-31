using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos.Users;

public record RegisterResponse(
    Guid Id,
    string Username,
    DateTime CreatedAt
);
