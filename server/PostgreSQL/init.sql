
-- Таблица брендов товаров
CREATE TABLE brands (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL
);

-- Таблица категорий товаров
CREATE TABLE category (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL
);

-- Таблица магазинов и складов
CREATE TABLE stores (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    address TEXT NOT NULL
);


-- Таблица товаров
CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    brand_id INT NOT NULL,
    category_id INT NOT NULL,
    image BYTEA,
    FOREIGN KEY (brand_id) REFERENCES brands(id),
    FOREIGN KEY (category_id) REFERENCES category(id)
);


-- Таблица клиентов
CREATE TABLE customers (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    contact_info TEXT,
    purchased_count INT NOT NULL DEFAULT 0
);

-- Таблица поставщиков
CREATE TABLE suppliers (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    contact_info TEXT,
    restocked_count INT NOT NULL DEFAULT 0
);


-- Таблица сотрудников 
CREATE TABLE employees (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    position VARCHAR(100),
    store_id INT NOT NULL,
    FOREIGN KEY (store_id) REFERENCES stores(id)
);


-- Таблица наличия товаров
CREATE TABLE stock (
    id SERIAL PRIMARY KEY,
    product_id INT NOT NULL,
    store_id INT NOT NULL,
    quantity INT NOT NULL DEFAULT 0,
    FOREIGN KEY (product_id) REFERENCES products(id),
    FOREIGN KEY (store_id) REFERENCES stores(id),
    UNIQUE (product_id, store_id)
);


-- Таблица цен на товары
CREATE TABLE product_prices (
    id SERIAL PRIMARY KEY,
    product_id INT NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    valid_from DATE NOT NULL,
    valid_to DATE NOT NULL,
    FOREIGN KEY (product_id) REFERENCES products(id),
    CHECK (valid_from <= valid_to)
);

-- Таблица продаж
CREATE TABLE sales (
    id SERIAL PRIMARY KEY,
    customer_id INT,
    store_id INT NOT NULL,
    employee_id INT NOT NULL,
    sale_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    discount_percentage DECIMAL(5,2) DEFAULT 0,
    items_amount INT NOT NULL DEFAULT 1,
    total_amount DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (store_id) REFERENCES stores(id),
    FOREIGN KEY (employee_id) REFERENCES employees(id),
    FOREIGN KEY (customer_id) REFERENCES customers(id)
);

-- Таблица товаров в заказе
CREATE TABLE sale_items (
    id SERIAL PRIMARY KEY,
    sale_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    unit_price DECIMAL(10,2) NOT NULL,
    total_price DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (sale_id) REFERENCES sales(id),
    FOREIGN KEY (product_id) REFERENCES products(id)
);

-- Таблица закупок
CREATE TABLE restocks (
    id SERIAL PRIMARY KEY,
    supplier_id INT NOT NULL,
    store_id INT NOT NULL,
    restock_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    items_amount INT NOT NULL DEFAULT 1,
    total_amount DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (supplier_id) REFERENCES suppliers(id),
    FOREIGN KEY (store_id) REFERENCES stores(id)
);

-- Таблица товаров в закупке
CREATE TABLE restock_items (
    id SERIAL PRIMARY KEY,
    restock_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    unit_price DECIMAL(10,2) NOT NULL,
    total_price DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (restock_id) REFERENCES restocks(id),
    FOREIGN KEY (product_id) REFERENCES products(id)
);

-- Таблица скидок
CREATE TABLE discounts (
    id SERIAL PRIMARY KEY,
    min_amount INT NOT NULL,
    discount_percentage DECIMAL(5,2) NOT NULL
);

-- Таблица перемещений товаров
CREATE TABLE transfers (
    id SERIAL PRIMARY KEY,
    from_store_id INT NOT NULL,
    to_store_id INT NOT NULL,
    transfer_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (from_store_id) REFERENCES stores(id),
    FOREIGN KEY (to_store_id) REFERENCES stores(id)
);

-- Таблица товаров в перемещениях
CREATE TABLE transfer_items (
    id SERIAL PRIMARY KEY,
    transfer_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    FOREIGN KEY (transfer_id) REFERENCES transfers(id),
    FOREIGN KEY (product_id) REFERENCES products(id)
);


-- Заполнение таблицы брендов
INSERT INTO brands (name) VALUES
    ('Samsung'),
    ('Apple'),
    ('Xiaomi'),
    ('LG'),
    ('Bosch');

-- Заполнение таблицы категорий
INSERT INTO category (name) VALUES
    ('Смартфоны'),
    ('Ноутбуки'),
    ('Телевизоры'),
    ('Холодильники'),
    ('Стиральные машины');

-- Заполнение таблицы товаров
INSERT INTO products (name, brand_id, category_id) VALUES
    ('Galaxy S23', 1, 1),
    ('iPhone 14', 2, 1),
    ('Redmi Note 12', 3, 1),
    ('OLED TV', 4, 3),
    ('Side-by-Side Холодильник', 5, 4),
    ('Ноутбук ProBook', 5, 2);

-- Заполнение таблицы магазинов
INSERT INTO stores (name, address) VALUES
    ('Магазин на Ленина', 'ул. Ленина, 15'),
    ('Магазин в ТЦ', 'ТЦ Центральный, 2 этаж'),
    ('Склад на окраине', 'Промышленная зона, склад 7');

-- Заполнение таблицы клиентов
INSERT INTO customers (name, contact_info) VALUES
    ('Иван Иванов', 'ivan@example.com, +79001234567'),
    ('Петр Петров', 'petr@example.com, +79112345678'),
    ('Анна Смирнова', 'anna@example.com, +79223456789');

-- Заполнение таблицы поставщиков
INSERT INTO suppliers (name, contact_info) VALUES
    ('Поставщик Электроники', '+74951112233, поставки@electro.com'),
    ('Импортер Техники', '+78124445566, import@tech.net');

-- Заполнение таблицы сотрудников
INSERT INTO employees (name, position, store_id) VALUES
    ('Елена Васильева', 'Продавец-консультант', 1),
    ('Михаил Кузнецов', 'Менеджер магазина', 1),
    ('Ольга Петрова', 'Продавец-консультант', 2),
    ('Сергей Иванов', 'Кладовщик', 3);

-- Заполнение таблицы наличия товаров (stock)
INSERT INTO stock (product_id, store_id, quantity) VALUES
    (1, 1, 20), -- Galaxy S23 в магазине на Ленина
    (2, 1, 15), -- iPhone 14 в магазине на Ленина
    (3, 2, 30), -- Redmi Note 12 в магазине в ТЦ
    (4, 2, 5),  -- OLED TV в магазине в ТЦ
    (5, 3, 100), -- Side-by-Side Холодильник на складе
    (6, 3, 50);  -- Ноутбук ProBook на складе

-- Заполнение таблицы цен на товары (product_prices)
INSERT INTO product_prices (product_id, price, valid_from, valid_to) VALUES
    (1, 79990.00, '2025-01-01', '2026-01-01'), -- Цена Galaxy S23
    (2, 99990.00, '2025-01-01', '2026-01-01'), -- Цена iPhone 14
    (3, 24990.00, '2025-01-01', '2026-01-01'), -- Цена Redmi Note 12
    (4, 129990.00, '2025-01-01', '2026-01-01'), -- Цена OLED TV
    (5, 89990.00, '2025-01-01', '2026-01-01'), -- Цена Side-by-Side Холодильник
    (6, 59990.00, '2025-01-01', '2026-01-01'); -- Цена Ноутбук ProBook

-- Заполнение таблицы продаж (sales)
INSERT INTO sales (customer_id, store_id, employee_id, sale_date, discount_percentage, items_amount, total_amount) VALUES
    (1, 1, 1, '2024-10-26 10:00:00', 0.05, 2, 151980.50), -- Продажа для Ивана
    (2, 2, 3, '2024-10-26 14:30:00', 0, 1, 24990.00),   -- Продажа для Петра
    (3, 1, 2, '2024-10-27 11:15:00', 0.10, 1, 71991.00); -- Продажа для Анны

-- Заполнение таблицы товаров в заказе (sale_items)
INSERT INTO sale_items (sale_id, product_id, quantity, unit_price, total_price) VALUES
    (1, 1, 1, 79990.00, 79990.00), -- Galaxy S23 в заказе 1
    (1, 2, 1, 99990.00, 99990.00), -- iPhone 14 в заказе 1
    (2, 3, 1, 24990.00, 24990.00), -- Redmi Note 12 в заказе 2
    (3, 1, 1, 79990.00, 71991.00); -- Galaxy S23 в заказе 3 (с скидкой)

-- Заполнение таблицы закупок (restocks)
INSERT INTO restocks (supplier_id, store_id, restock_date, items_amount, total_amount) VALUES
    (1, 3, '2024-10-25 15:00:00', 150, 1200000.00), -- Закупка на склад от поставщика 1
    (2, 3, '2024-10-26 09:00:00', 100, 500000.00);   -- Еще одна закупка на склад

-- Заполнение таблицы товаров в закупке (restock_items)
INSERT INTO restock_items (restock_id, product_id, quantity, unit_price, total_price) VALUES
    (1, 1, 50, 60000.00, 3000000.00), -- Galaxy S23 в закупке 1
    (1, 3, 100, 20000.00, 2000000.00), -- Redmi Note 12 в закупке 1
    (2, 5, 50, 80000.00, 4000000.00),  -- Холодильники в закупке 2
    (2, 6, 50, 40000.00, 2000000.00);  -- Ноутбуки в закупке 2

-- Заполнение таблицы скидок (discounts)
INSERT INTO discounts (min_amount, discount_percentage) VALUES
    (100000, 0.05), -- Скидка 5% при покупке от 100000
    (200000, 0.10); -- Скидка 10% при покупке от 200000

-- Заполнение таблицы перемещений товаров (transfers)
INSERT INTO transfers (from_store_id, to_store_id) VALUES
    (3, 1), -- Перемещение со склада в магазин на Ленина
    (3, 2); -- Перемещение со склада в магазин в ТЦ

-- Заполнение таблицы товаров в перемещениях (transfer_items)
INSERT INTO transfer_items (transfer_id, product_id, quantity) VALUES
    (1, 1, 10), -- 10 Galaxy S23 перемещены в магазин на Ленина
    (1, 2, 5),  -- 5 iPhone 14 перемещены в магазин на Ленина
    (2, 3, 15), -- 15 Redmi Note 12 перемещены в магазин в ТЦ
    (2, 4, 3);  -- 3 OLED TV перемещены в магазин в ТЦ







