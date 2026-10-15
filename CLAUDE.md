# Bodeglook

Bodeglook es una red social gastronómica argentina. Los usuarios organizan salidas con amigos (planes con votación), descubren restaurantes y bodegas y dejan reseñas honestas, sin canjes.

Repo: Uri-Cherno444/Bodeglook

## Diseño en Figma

- UI Kit: https://www.figma.com/design/s1xP4urCzZ6f4WrvJ0EMY3/Bodeglook?node-id=623-213
- Prototipo en alta (7 pantallas): https://www.figma.com/design/s1xP4urCzZ6f4WrvJ0EMY3/Bodeglook?node-id=5644-147
  - 01 · Index, 02 · Iniciar Sesión, 03 · Inicio, 04 · Detalle + Reseña, 05 · Perfil y Amigos, 06 · Crear Plan, 07 · Descubrir Restaurantes

## Stack obligatorio (no cambiar)

- ASP.NET Core MVC (.NET 9), C#, un solo proyecto con vistas Razor.
- SQL Server + Dapper. Sin Entity Framework.
- Session para el usuario logueado.
- Front: Razor + CSS + JavaScript sin frameworks, con fetch y JSON. Sin React ni Bootstrap.

## Estructura

- Solo `Controllers/`, `Models/`, `Views/` y `wwwroot/`, más `BD.sql` en la raíz.
- No crear carpetas nuevas (Componentes, Helpers, Filtros, Services, etc.) sin preguntar.
- El acceso a datos va en una clase `BD` estática dentro de `Models/`, con un método por consulta usando Dapper.
- La connection string está en `appsettings.json` (`ConnectionStrings:Bodeglook`), nunca escrita en `BD.cs`.
- Código simple, de nivel escolar: nada de interfaces, inyección de dependencias ni patrones avanzados.

## Reglas

1. Imitar el Figma. No inventar pantallas, campos, textos ni funciones. Si falta un dato, preguntar antes de decidir.
2. Todo en español: textos de la interfaz, clases y métodos.
3. La base la diseñamos nosotros. No cambiar tablas ni columnas sin preguntar.
4. Las contraseñas se guardan con hash, nunca en texto plano.
5. Al terminar cada tarea, decir qué archivos se tocaron y cómo probarlo.
