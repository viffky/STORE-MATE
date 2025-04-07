# StoreMate

StoreMate — это лёгкий веб-интерфейс для администрирования базы данных магазина, а также десктоп приложение WinForms (.Net 8.0).
Работает на Flask + PostgreSQL, использует только HTML+JS без отдельного фронтенда.


## Инструкция по установке

1. **Клонируйте репозиторий**  
   ```bash
   git clone https://github.com/KeyKerGG/StoreMate
   cd StoreMate
   ```

2. **Соберите и запустите с помощью Docker**  
   Убедитесь, что у вас установлены и запущены Docker и Docker Compose. Затем выполните команду:
   ```bash
   docker-compose up --build
   ```

3. **Доступ к приложению**
   - Веб-админка: [http://localhost:5000](http://localhost:5000)
   - API эндпоинты: доступны по пути `/api/...`
   - PostgreSQL база данных: доступна через Docker сеть на порту `5432`
   - Десктопный клиент WinForms: запустите из папки `/client` (отдельный проект .NET)

