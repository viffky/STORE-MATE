from flask import Flask, render_template, jsonify, request
import psycopg2
from psycopg2.extras import RealDictCursor
import os
import base64

app = Flask(__name__, template_folder="Web")
DATABASE_URL = os.environ.get('DATABASE_URL', 'postgresql://postgres:password@db:5432/storeMate')


def get_db_connection():
    return psycopg2.connect(DATABASE_URL)


def insert_and_return_id(cur, table, name):
    cur.execute(f"INSERT INTO {table} (name) VALUES (%s) RETURNING id", (name,))
    return cur.fetchone()[0]


@app.route('/')
def index():
    conn = get_db_connection()
    cur = conn.cursor()
    cur.execute('''
        SELECT p.id, p.name, b.name, c.name
        FROM products p
        JOIN brands b ON p.brand_id = b.id
        JOIN category c ON p.category_id = c.id
    ''')
    products = cur.fetchall()
    cur.close()
    conn.close()
    return render_template('index.html', products=products)


@app.route('/api/products', methods=['GET'])
def get_products():
    conn = get_db_connection()
    cur = conn.cursor()
    cur.execute('''
        SELECT
            p.id,
            p.name,
            b.name AS brand,
            c.name AS category,
            p.image,
            pp.price
        FROM products p
        JOIN brands b ON p.brand_id = b.id
        JOIN category c ON p.category_id = c.id
        LEFT JOIN LATERAL (
            SELECT price
            FROM product_prices
            WHERE product_id = p.id
              AND CURRENT_DATE BETWEEN valid_from AND valid_to
            ORDER BY valid_from DESC
            LIMIT 1
        ) pp ON true
    ''')
    rows = cur.fetchall()
    cur.close()
    conn.close()

    products = []
    for row in rows:
        image_data = row[4]
        image_base64 = base64.b64encode(image_data).decode('utf-8') if image_data else None

        products.append({
            'id': row[0],
            'name': row[1],
            'brand': row[2],
            'category': row[3],
            'image': image_base64,
            'price': row[5] if row[5] is not None else 'не указана'
        })

    return jsonify(products)


@app.route('/api/products', methods=['POST'])
def add_product():
    data = request.get_json()
    required_fields = ['id', 'name', 'brand', 'category']

    for field in required_fields:
        if not data.get(field):
            return jsonify({'error': f'Missing field: {field}'}), 400

    product_id = data['id']
    name = data['name']
    brand = data['brand']
    category = data['category']
    image_data = base64.b64decode(data['image']) if data.get('image') else None

    conn = get_db_connection()
    cur = conn.cursor()

    # Проверка: существует ли уже товар с таким ID
    cur.execute("SELECT id FROM products WHERE id = %s", (product_id,))
    if cur.fetchone():
        return jsonify({'error': 'Product with this ID already exists'}), 400

    # Получить или создать brand_id
    cur.execute("SELECT id FROM brands WHERE name = %s", (brand,))
    brand_row = cur.fetchone()
    brand_id = brand_row[0] if brand_row else insert_and_return_id(cur, 'brands', brand)

    # Получить или создать category_id
    cur.execute("SELECT id FROM category WHERE name = %s", (category,))
    category_row = cur.fetchone()
    category_id = category_row[0] if category_row else insert_and_return_id(cur, 'category', category)

    # Добавить товар
    cur.execute('''
        INSERT INTO products (id, name, brand_id, category_id, image)
        VALUES (%s, %s, %s, %s, %s)
    ''', (product_id, name, brand_id, category_id, image_data))

    conn.commit()
    cur.close()
    conn.close()

    return jsonify({'status': 'created'}), 201


@app.route('/api/products', methods=['PUT'])
def update_product():
    data = request.get_json()
    required_fields = ['id', 'name', 'brand', 'category']

    for field in required_fields:
        if not data.get(field):
            return jsonify({'error': f'Missing field: {field}'}), 400

    product_id = data['id']
    name = data['name']
    brand = data['brand']
    category = data['category']
    image_data = base64.b64decode(data['image']) if data.get('image') else None

    conn = get_db_connection()
    cur = conn.cursor()

    # Проверка существования товара
    cur.execute("SELECT id FROM products WHERE id = %s", (product_id,))
    if not cur.fetchone():
        return jsonify({'error': 'Product not found'}), 404

    # Получить или создать brand_id
    cur.execute("SELECT id FROM brands WHERE name = %s", (brand,))
    brand_row = cur.fetchone()
    brand_id = brand_row[0] if brand_row else insert_and_return_id(cur, 'brands', brand)

    # Получить или создать category_id
    cur.execute("SELECT id FROM category WHERE name = %s", (category,))
    category_row = cur.fetchone()
    category_id = category_row[0] if category_row else insert_and_return_id(cur, 'category', category)

    # Обновить товар
    cur.execute('''
        UPDATE products SET name = %s, brand_id = %s, category_id = %s, image = %s
        WHERE id = %s
    ''', (name, brand_id, category_id, image_data, product_id))

    conn.commit()
    cur.close()
    conn.close()

    return jsonify({'status': 'updated'}), 200


@app.route('/api/products/<int:product_id>', methods=['DELETE'])
def delete_product(product_id):
    conn = get_db_connection()
    cur = conn.cursor()

    cur.execute('DELETE FROM products WHERE id = %s', (product_id,))
    conn.commit()

    cur.close()
    conn.close()
    return jsonify({'message': f'Product {product_id} deleted successfully'})


@app.route('/api/<table>', methods=['GET'])
def get_all(table):
    conn = get_db_connection()
    cur = conn.cursor(cursor_factory=RealDictCursor)
    cur.execute(f'SELECT * FROM {table}')
    items = cur.fetchall()
    cur.close()
    conn.close()
    return jsonify(items)


@app.route('/api/<table>', methods=['POST'])
def add_item(table):
    data = request.json
    keys = data.keys()
    values = [data[k] for k in keys]

    placeholders = ', '.join(['%s'] * len(values))
    columns = ', '.join(keys)

    conn = get_db_connection()
    cur = conn.cursor()
    cur.execute(f'INSERT INTO {table} ({columns}) VALUES ({placeholders})', values)
    conn.commit()
    cur.close()
    conn.close()
    return jsonify({'message': f'Added to {table}'})


@app.route('/api/<table>/<int:item_id>', methods=['PUT'])
def update_item(table, item_id):
    data = request.json
    keys = data.keys()
    values = [data[k] for k in keys]

    assignments = ', '.join([f"{key} = %s" for key in keys])

    conn = get_db_connection()
    cur = conn.cursor()
    cur.execute(f'UPDATE {table} SET {assignments} WHERE id = %s', values + [item_id])
    conn.commit()
    cur.close()
    conn.close()
    return jsonify({'message': f'Updated {table} item {item_id}'})


@app.route('/api/<table>/<int:item_id>', methods=['DELETE'])
def delete_item(table, item_id):
    conn = get_db_connection()
    cur = conn.cursor()
    cur.execute(f'DELETE FROM {table} WHERE id = %s', (item_id,))
    conn.commit()
    cur.close()
    conn.close()
    return jsonify({'message': f'Deleted from {table} id {item_id}'})


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)
