// ユーザー登録処理のコード
public class UserApplicationService
{
  private readonly IUserFactory userFactory;
  private readonly IUserRepository userRepository;
  private readonly UserService userService;

  // 略

  public void Register(UserRegisterCommand command)
  {
    var userName = new UserName(command.Name);
    var user = userFactory.Create(userName);

    if (userService.Exists(user))
    {
      throw new CanNotRegisterUserException(user, "ユーザーは既に存在しています");
    }

    // アプリケーション側でのバリデーションのみでは
    // 同時に登録処理を行った場合に同じ名前のユーザーが登録できてしまう
    userRepository.Save(user);
  }
}

// ユニークキー制約で重複しないことが担保され、重複確認が不要になる
public class UserApplicationService
{
  private readonly IUserFactory userFactory;
  private readonly IUserRepository userRepository;

  // 略

  public void Register(UserRegisterCommand command)
  {
    var userName = new UserName(command.Name);
    var user = userFactory.Create(userName);

    userRepository.Save(user);
  }
}


// 重複チェックの対象をメールアドレスにしてしまった
public class UserService
{
  private readonly IUserRepository userRepository;

  // 略

  public bool Exists(User user)
  {
    var duplicatedUser = userRepository.Find(user.Mail);
    return duplicatedUser != null;
  }
}

// トランザクションが開始されたコネクションを利用する
public class userRepository: IUserRepository
{
  // 引き渡されたデータベースのコネクション
  private readonly SqlConnection connection;

  public UserRepository(SqlConnection connection)
  {
    this.connection = connection;
  }

  public void Save(User user, SqlTransation transaction = null)
  {
    using(var command = connection.CreateCommand())
    {
      if (transaction != null)
      {
        command.Transaction = transaction;
      }
      command.CommandText = @"
MERGE INTO users
  USING (
    SELECT @id AS id, @name AS name
  ) AS data
  ON users.id = data.id
WHEN MATCHED THEN
  UPDATE SET name = data.name
WHEN NOT MATCHED THEN
  INSERT (id, name)
  VALUES (data.id, data.name);
";
      command.Parameters.Add(new SQLParameter("@id, user.Id.Value"));
      command.Parameters.Add(new SQLParameter("@name, user.Name.Value"));

      command.ExecuteNonQuery();
    }
  }
}

// コンストラクタでトランザクションを受け取る
public class UserApplicatoinService
{
  // このコネクションはリポジトリに保存しているものと同じもの
  private readonly SqlConnection connection;
  private readonly UserService userService;
  private readonly IUserFactory userFactory;
  private readonly IUserRepository userRepository;

  public UserApplicationService(SqlConnection connection, UserService userService, IUserFactory userFactory, IUserRepository userRepository)
  {
    this.connection = connection;
    this.userService = userService;
    this.userFactory = userFactory;
    this.userRepository = userRepository;
  }

  // 略
}

// トラザクションを利用するようにユーザ登録処理を書き換える
public class UserApplicationService
{
  UserApplicationServiceが特定の技術に依存してしまっているå
  private readonly SqlConnection connection;
  private readonly UserService userService;
  private readonly IUserFactory userFactory;
  private readonly IUserRepository userRepository;

  // 略

  public void Register(UserRegisterCommand command)
  {
    var userName = new UserName(command.Name);
    var user = userFactory.Create(userName);

    if (userService.Exists(user))
    {
      throw new CanNotRegisterUserException(user, "ユーザは既に存在しています。");
    }

    userRepository.Save(user, transaction);
    // 完了時にコミットを行う
    transaction.Commit();
  }

  // 略
}


