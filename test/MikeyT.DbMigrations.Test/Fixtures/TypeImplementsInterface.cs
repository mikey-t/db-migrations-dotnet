namespace MikeyT.DbMigrations.Test.Fixtures;

public interface IMyNonGenericInterface { }
public class MyClass : IMyNonGenericInterface { }

public interface IMyGenericInterface<T> { }
public class MyGenericClass : IMyGenericInterface<string> { }

public class MyClassWithNoInterface { }
