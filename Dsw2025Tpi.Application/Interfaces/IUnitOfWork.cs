namespace Dsw2025Tpi.Application.Interfaces;

/// <summary>
/// Unidad de trabajo: agrupa los cambios acumulados por los repositorios y los
/// confirma de forma atomica.
///
/// Los repositorios NO confirman por su cuenta: solo registran los cambios en el
/// contexto. Es el caso de uso el que decide cuando termina la operacion e invoca
/// <see cref="SaveChangesAsync"/>. Esa es la unica forma de que una operacion que
/// toca varias entidades sea "todo o nada".
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Confirma en una sola transaccion todos los cambios pendientes del contexto
    /// de dominio. EF Core envuelve internamente cada SaveChanges en una
    /// transaccion, por lo que basta para garantizar atomicidad cuando la
    /// operacion involucra un unico contexto.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta una operacion que abarca ambos contextos (dominio e Identity)
    /// dentro de una unica transaccion explicita. Si la accion lanza, se revierte
    /// todo lo hecho en los dos contextos.
    ///
    /// Solo es necesario cuando la operacion cruza los dos contextos, como el
    /// registro de usuario. Para operaciones de un solo contexto alcanza con
    /// <see cref="SaveChangesAsync"/>.
    /// </summary>
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default);
}
