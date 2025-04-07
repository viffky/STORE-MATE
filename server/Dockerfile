# Используем Python 3.9 как базовый образ
FROM python:3.9-slim

# Устанавливаем рабочую директорию
WORKDIR /app

# Копируем зависимости
COPY requirements.txt /app/
RUN pip install -r requirements.txt

# Копируем проект
COPY . /app/

# Команда для запуска приложения
CMD ["flask", "run", "--host=0.0.0.0"]