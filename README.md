# Blazor .NET 8 | Custom Identity

Olá Dev! 😎

Esse projeto servirá como base para projetos Frontend/FullStack usando Blazor .NET 8.

O foco da aplicação é gerenciar informações dos perfis de acesso.

Obs: No exemplo, a foto de perfil será salva no banco de dados, apenas para demonstração.

Para uma aplicação real, o recomendado é salvar imagens em uma storage na nuvem. No banco, apenas gravar o link para a imagem.

## Novo Projeto

Vamos iniciar criando um projeto blazor server com suporte ao **Identity**, que fornece solução de login completa:

```csharp
dotnet new blazor -o BlazorCustomIdentity -int Auto -au Individual

//  Parâmetros do comando:
//  -int Auto -> Adiciona opções de interatividade Server e WebAssembly, gerando 2 projetos na Solution.
//  -au -> Adiciona a autenticação do usuário via Identity.
```

**Arquitetura:** Desenvolvimento orientado a dados - Data Driven Design (DDD):

**Estrutura:**

- **BlazorCustomIdentity**
  - `Components`
  - `Data`
  - `wwwroot`

Resumo:

- **Components** -> Páginas e components gerados no projeto. Aqui temos acesso as configurações do Identity.
- **Data** -> Contexto do banco de dados, refletindo na aplicação a estrutura de campos e tabelas.
- **Pages** -> Paginas e componentes para visualizar e interagir com os dados.
- **wwwroot** -> Arquivos estáticos como scripts, css e imagens.

## Personalizando o Identity

Vamos personalizar o código gerado na criação do projeto. Créditos ao Dev pelo [tutorial][TutorialPureSource], servindo de base.

Foco nas alterações:

**Data** -> Adição de novas propriedades ao ApplicationUser.

**Data** -> Personalização do ApplicationDbContext.

**Components** -> Account -> Pages -> Register.razor. Alteração no formulário de cadastro do usuário.

**Components** -> Layout -> NavMenu.razor. Alteração para exibir o Nome do usuário, ao invés do email.

**Components** -> Pages -> Account -> Manage -> Index.razor. Alteração permitindo o usuário editar suas informações.

**Components** -> Pages -> Account -> Manage -> Photo.razor. Nova pagina para upload de foto de perfil do usuário.

**Components** -> Account -> Shared -> ManageNavMenu.razor. Inclusão da da rota para pagina de foto de perfil.

### ApplicationUser

1. Agora atualize a entidade Data -> ApplicationUser, adicionando novas Propriedades para o usuário.

   ```csharp
   using Microsoft.AspNetCore.Identity;

   namespace BlazorTodoApp.Data;

   public class ApplicationUser : IdentityUser
   {
       // Adicione os campos que forem relevantes
       public string? FirstName { get; set; }
       public string? LastName { get; set; }
       public int UsernameChangeLimit { get; set; } = 10;
       public string? ProfilePictureBase64 { get; set; }
   }
   ```

### AppDbContext

1. Atualize a entidade Data -> ApplicationDbContext para gerar as Tabelas com a seguinte identificação.

   ```csharp
   using Microsoft.AspNetCore.Identity;
   using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
   using Microsoft.EntityFrameworkCore;

   namespace BlazorTodoApp.Data;

   // Renomeada classe para AppDbContext
   public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
   {
       protected override void OnModelCreating(ModelBuilder builder)
       {
           base.OnModelCreating(builder);
           builder.HasDefaultSchema("Identity");
           builder.Entity<IdentityUser>(entity =>
           {
               entity.ToTable("User");
           });
           builder.Entity<IdentityRole>(entity =>
           {
               entity.ToTable("Role");
           });
           builder.Entity<IdentityUserRole<string>>(entity =>
           {
               entity.ToTable("UserRoles");
           });
           builder.Entity<IdentityUserClaim<string>>(entity =>
           {
               entity.ToTable("UserClaims");
           });
           builder.Entity<IdentityUserLogin<string>>(entity =>
           {
               entity.ToTable("UserLogins");
           });
           builder.Entity<IdentityRoleClaim<string>>(entity =>
           {
               entity.ToTable("RoleClaims");
           });
           builder.Entity<IdentityUserToken<string>>(entity =>
           {
               entity.ToTable("UserTokens");
           });
       }
   }

   // Crie uma migração e atualize a base para refletir as mudanças no código

   // Execute no terminal: dotnet ef migrations add "Renamed Identity Table Names"
   // Depois execute: dotnet ef database update
   ```

### Register.razor

1. Atualize o InputModel em Components -> Account -> Pages Register.razor

   ```csharp
   private sealed class InputModel
   {
     [Required]
     [EmailAddress]
     [Display(Name = "Email")]
     public string Email { get; set; } = "";

     [Required]
     [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
     [DataType(DataType.Password)]
     [Display(Name = "Password")]
     public string Password { get; set; } = "";

     [DataType(DataType.Password)]
     [Display(Name = "Confirm password")]
     [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
     public string ConfirmPassword { get; set; } = "";

     // Novos campos adicionados
     [Required]
     [Display(Name = "First Name")]
     public string FirstName { get; set; } = null!;

     [Required]
     [Display(Name = "Last Name")]
     public string LastName { get; set; } = null!;
   }
   ```

2. Agora modifique o método RegisterUser, criando um objeto ApplicationUser com as propriedades adicionais.

   ```csharp
   // Obs: adicione ao topo da pagina a importação do @using System.Net.Mail
   public async Task RegisterUser(EditContext editContext)
       {
           MailAddress address = new MailAddress(Input.Email);
           string userName = address.User;

           var user = new ApplicationUser()
           {
               UserName = userName,
               Email = Input.Email,
               FirstName = Input.FirstName,
               LastName = Input.LastName
           };

           await UserStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
           //... Continua o mesmo código
       }
   ```

3. Atualize o EditForm

   ```csharp
   <EditForm Model="Input" asp-route-returnUrl="@ReturnUrl" method="post" OnValidSubmit="RegisterUser" FormName="register">
      <DataAnnotationsValidator />
      <h2>Create a new account.</h2>
      <hr />
      <ValidationSummary class="text-danger" role="alert" />
      <div class="form-floating mb-3">
          <InputText id="firstname" @bind-Value="Input.FirstName" class="form-control" aria-required="true" placeholder="John"/>
          <label for="firstname">Nome</label>
          <ValidationMessage For="() => Input.FirstName" class="text-danger" />
      </div>
      <div class="form-floating mb-3">
          <InputText id="lastname" @bind-Value="Input.LastName" class="form-control" aria-required="true" placeholder="Doe"/>
          <label for="lastname">Sobrenome</label>
          <ValidationMessage For="() => Input.LastName" class="text-danger" />
      </div>
      <div class="form-floating mb-3">
          <InputText @bind-Value="Input.Email" class="form-control" autocomplete="username" aria-required="true" placeholder="name@example.com" />
          <label for="email">Email</label>
          <ValidationMessage For="() => Input.Email" class="text-danger" />
      </div>
      <div class="form-floating mb-3">
          <InputText type="password" @bind-Value="Input.Password" class="form-control" autocomplete="new-password" aria-required="true" placeholder="password" />
          <label for="password">Senha</label>
          <ValidationMessage For="() => Input.Password" class="text-danger" />
      </div>
      <div class="form-floating mb-3">
          <InputText type="password" @bind-Value="Input.ConfirmPassword" class="form-control" autocomplete="new-password" aria-required="true" placeholder="password" />
          <label for="confirm-password">Confirme a senha</label>
          <ValidationMessage For="() => Input.ConfirmPassword" class="text-danger" />
      </div>
      <button type="submit" class="w-100 btn btn-lg btn-primary">Registrar</button>
   </EditForm>
   ```

### NavMenu

1. Altere o Components -> Layout -> NavMenu.razor

   ```csharp
   //Todo da pagina
   @implements IDisposable

   @using BlazorCustomIdentity.Data
   @using Microsoft.AspNetCore.Identity

   @inject NavigationManager NavigationManager
   @inject UserManager<ApplicationUser> UserManager

   // Agora no AuthorizeView, altere o NavLink conforme indicado
   <AuthorizeView>
      <Authorized>
          <div class="nav-item px-3">
              <NavLink class="nav-link" href="Account/Manage">
                  <span class="bi bi-person-fill-nav-menu" aria-hidden="true"></span>
                  // Código para exibir o nome do usuário ou login
                  @(UserManager?.GetUserAsync(context.User)?.Result?.FirstName ?? context.User.Identity?.Name)
              </NavLink>
          </div>
   //... mantenha demais estruturas
   </AuthorizeView>
   ```

Após essas modificações, temos o tela do usuário com os novos campos no momento de criar uma conta.

![CustomLoginFirstAndLastName][CustomLoginFirstAndLastName]

E após autenticar, o primeiro nome do usuário é exibido ao invés do Email.

![LoginNavName][LoginNavName]

## Edição de Perfil - adicionando novos campos para o usuário

1. Altere a pagina Components -> Pages -> Account -> Manage -> Index.razor.

   ```csharp
   // Adicione os campos no InputModel
   private sealed class InputModel
   {
       [Display(Name = "Nome")]
       public string? FirstName { get; set; }

       [Display(Name = "Sobrenome")]
       public string? LastName { get; set; }

       [Display(Name = "Usuário")]
       public string? Username { get; set; }

       [Phone]
       [Display(Name = "Celular")]
       public string? PhoneNumber { get; set; }
   }

   // Altere o método de inicialização, passando as novas variáveis
   private ApplicationUser user = default!;
   private string? username;
   private string? firstName;
   private string? lastName;
   private string? phoneNumber;

   protected override async Task OnInitializedAsync()
    {
        user = await UserAccessor.GetRequiredUserAsync(HttpContext);
        username = user.UserName;
        firstName = user.FirstName;
        lastName = user.LastName;
        phoneNumber = user.PhoneNumber;

        Input.FirstName ??= firstName;
        Input.LastName ??= lastName;
        Input.PhoneNumber ??= phoneNumber;

       // Obs: ao usar o operador de coalescência nula ??=, reduzimos linhas no código.
       // Veja como seria sem ele cada verificação de propriedade e atribuição:
       // if (Input.FirstName is null)
       // {
       //   Input.FirstName = firstName;
       // }
    }
   ```

2. Agora altere o form, inserindo os novos campos.

   ```csharp
   <div class="row">
       <div class="col-md-6">
           <EditForm Model="Input" FormName="profile" OnValidSubmit="OnValidSubmitAsync" method="post">
               <DataAnnotationsValidator />
               <ValidationSummary class="text-danger" role="alert" />

               <div class="form-floating mb-3">
                   <input type="text" value="@username" class="form-control"
                       placeholder="Please choose your username."
                       disabled />
                   <label for="username" class="form-label">Username</label>
               </div>

               <div class="form-floating mb-3">
                   <InputText @bind-Value="Input.FirstName" class="form-control"
                       placeholder="Please enter your first name." />
                   <label for="firstname" class="form-label">Nome</label>
                   <ValidationMessage For="() => Input.FirstName" class="text-danger" />
               </div>

               <div class="form-floating mb-3">
                   <InputText @bind-Value="Input.LastName" class="form-control"
                       placeholder="Please enter your last name." />
                   <label for="lastname" class="form-label">Sobrenome</label>
                   <ValidationMessage For="() => Input.LastName" class="text-danger" />
               </div>

               <div class="form-floating mb-3">
                   <InputText @bind-Value="Input.PhoneNumber" class="form-control"
                       placeholder="Please enter your phone number." />
                   <label for="phone-number" class="form-label">Celular</label>
                   <ValidationMessage For="() => Input.PhoneNumber" class="text-danger" />
               </div>

               <button type="submit" class="w-100 btn btn-lg btn-primary">Save</button>
           </EditForm>
       </div>
   </div>
   ```

3. Atualize o método de envio do form, para salvar os dados do usuário.

   ```csharp
   private async Task OnValidSubmitAsync()
    {
        var setUserDataResult = await UserManager
           .SetUserDataAsync(user, Input.FirstName, Input.LastName, Input.PhoneNumber);

        if (!setUserDataResult.Succeeded)
        {
            RedirectManager.RedirectToCurrentPageWithStatus("Error: Failed to update user data.", HttpContext);
        }

        await SignInManager.RefreshSignInAsync(user);
        RedirectManager.RedirectToCurrentPageWithStatus("Your profile has been updated", HttpContext);
    }

   // Crie uma pasta Extensions em seu projeto.
   // Agora crie um método de extensão para utilizar o SetUserDataAsync da classe UserManager
   using BlazorCustomIdentity.Data;
   using Microsoft.AspNetCore.Identity;

   namespace BlazorCustomIdentity.Extensions;

   // Podemos criar vários métodos de extensão, criando a classe e métodos estáticos conforme esse exemplo
   public static class UserManagerExtension
   {
       public static async Task<IdentityResult> SetUserDataAsync(
               this UserManager<ApplicationUser> userManager,
               ApplicationUser user,
               string firstName,
               string lastName,
               string phoneNumber)
       {
           user.FirstName = firstName;
           user.LastName = lastName;
           user.PhoneNumber = phoneNumber;

           return await userManager.UpdateAsync(user);
       }
   }
   ```

Adicione a importação do namespace com a extensão **@using BlazorCustomIdentity.Extensions** ao Index.razor

O form de atualização ficará similar a esse.

![CustomUserDataForm][CustomUserDataForm]

### Upload foto do usuário

Seria muito mais prático adicionar a foto no form do perfil do usuário. Porém, ao utilizar a instrução @renderMode com o Identity, a aplicação para de funcionar.

Obs: Futuramente, pretendo aprimorar esse item.

1. Crie uma nova pagina chamada Photo.razor.

   ```csharp
   // Rota para pagina e processamento via servidor
   @page "/Account/Manage/Photo"
   @rendermode InteractiveServer

   @using System.ComponentModel.DataAnnotations
   @using Microsoft.AspNetCore.Identity
   @using BlazorCustomIdentity.Data
   @using BlazorCustomIdentity.Extensions

   // Recupera o usuário autenticado na aplicação
   @inject AuthenticationStateProvider AuthenticationStateProvider
   @inject UserManager<ApplicationUser> UserManager
   @inject NavigationManager NavigationManager

   // CSS para o botão de Carregamento
   <style>
       .custom-input-file {
           display: none;
       }

       .custom-file-upload {
           margin: 6px 0px;
           cursor: pointer;
           background: #b3b6b3;
           color: white;
           border: none;
           border-radius: 4px;
           text-align: center;
       }

       .custom-file-upload:hover {
           background: #707571;
       }
   </style>

   <h3>Photo</h3>

   <div class="row">
       <div class="col-md-6">
           // Exibe miniatura da foto de perfil
           @if (image != null)
           {
               <div class="form-group">
                   <img src="data:@image.ContentType;base64,@image.Base64data" />
               </div>
           }

           <div class="form-group">
               <label for="inputFile" class="w-50 btn btn-lg custom-file-upload">
                   <i class="bi bi-cloud-arrow-up-fill"></i> Carregar
               </label>

               <InputFile class="custom-input-file" OnChange="OnChange" accept="image/png, image/jpeg, image/gif"
                   id="inputFile" />
           </div>

           <button type="submit" @onclick="OnClickUploadAsync" class="w-50 btn btn-lg btn-primary">Salvar</button>
       </div>
   </div>

   @code {
       private ImageFile? image;

       private ApplicationUser user = default!;

       protected override async Task OnInitializedAsync()
       {
           var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
           var authUser = authState.User;

           // Verificação necessária para usar com o @renderMode. Assim, recupero o usuário autenticado
           if (authUser.Identity.IsAuthenticated)
               user = await UserManager.GetUserAsync(authUser);

           // Retorna a imagem atual do usuário, se houver
           if (user != null)
               if (!string.IsNullOrWhiteSpace(user.ProfilePictureBase64))
                   image = new ImageFile
                       {
                           Base64data = user.ProfilePictureBase64,
                           ContentType = "PNG",
                           FileName = "ProfilePicture"
                       };
       }

       // Ao carregar a imagem, cria um objeto image, com os dados convertidos para base64
       async Task OnChange(InputFileChangeEventArgs e)
       {
           var file = e.File;
           var resizedFile = await file.RequestImageFileAsync(file.ContentType, 128, 128);

           var buf = new byte[resizedFile.Size];
           using (var stream = resizedFile.OpenReadStream())
           {
               await stream.ReadAsync(buf, 0, buf.Length);
           }

           image = new ImageFile
               {
                   Base64data = Convert.ToBase64String(buf),
                   ContentType = file.ContentType,
                   FileName = file.Name
               };
       }

       async Task OnClickUploadAsync()
       {
           // Chama o método de extensão, passando o usuário e a imagem no padrão base64
           var setUserDataResult = await UserManager.SetUserPhotoAsync(user, image.Base64data);

           if (!setUserDataResult.Succeeded)
               NavigationManager.NavigateTo("/Account/Manage", true);

           NavigationManager.NavigateTo("/Account/Manage", true);
       }
   }
   ```

1. Atualize o UserManagerExtension, criando o método SetUserPhotoAsync.

   ```csharp
   // public static class UserManagerExtension
   // ... Códigos acima
       public static async Task<IdentityResult> SetUserPhotoAsync(
       this UserManager<ApplicationUser> userManager,
       ApplicationUser user, string profilePictureBase64)
       {
           user.ProfilePictureBase64 = profilePictureBase64;

           return await userManager.UpdateAsync(user);
       }
   ```

### ManageNavMenu

Vamos adicionar essa nova pagina ao menu de gerenciamento do usuário.

Components -> Account -> Shared -> ManageNavMenu.razor

```csharp
@using Microsoft.AspNetCore.Identity
@using BlazorCustomIdentity.Data

@inject SignInManager<ApplicationUser> SignInManager

<ul class="nav nav-pills flex-column">
    <li class="nav-item">
        <NavLink class="nav-link" href="Account/Manage" Match="NavLinkMatch.All">Profile</NavLink>
    </li>
    // Crie um item abaixo do profile, com a rota pagina Photo
    <li class="nav-item">
        <NavLink class="nav-link" href="Account/Manage/Photo">Photo</NavLink>
    </li>
    <li class="nav-item">
        <NavLink class="nav-link" href="Account/Manage/Email">Email</NavLink>
    </li>
// Demais códigos...
ul />
```

Perfil com a opção Foto

![CustomUserProfilePicture][CustomUserProfilePicture]

Após carregar a foto

![CustomUserProfilePictureThumb][CustomUserProfilePictureThumb]

Após salvar a foto, fiz o acesso ao SQLite para conferir a imagem convertida para Base64

![DbPictureBase64][DbPictureBase64]

### Bom, é isso aí. Bons estudos e bons códigos! 👍

[CustomLoginFirstAndLastName]: BlazorCustomIdentity/wwwroot/doc/custom-login.png
[TutorialPureSource]: https://puresourcecode.com/dotnet/blazor/custom-user-management-with-net8-and-blazor/
[LoginNavName]: BlazorCustomIdentity/wwwroot/doc/login-name-nav.png
[CustomUserDataForm]: BlazorCustomIdentity/wwwroot/doc/custom-user-data-form.png
[CustomUserProfilePicture]: BlazorCustomIdentity/wwwroot/doc/custom-user-profile-picture.png
[CustomUserProfilePictureThumb]: BlazorCustomIdentity/wwwroot/doc/custom-user-profile-picture-thumb.png
[DbPictureBase64]: BlazorCustomIdentity/wwwroot/doc/db-picture-base64.png
