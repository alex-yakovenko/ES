Feature: UpdateOrder

Adding and removing products for an existing Order

@tag1
Scenario: Add new product to Order
	Given All flows are configured
	And product with SKU "BOOK-1" and available quantity 350 is present in inventory
	And product with SKU "BOOK-2" and available quantity 140 is present in inventory
	And having placed order for "ABC ltd", date "11/12/2025", ID "O2025/1324" and products as follows:
	| Product ID | Quantity |
	| BOOK-1     |      150 |
	When updating order "O2025/1324" product changes as follows:
	| Product ID | Quantity |
	| BOOK-2     |     +120 |
	Then order "O2025/1324" has status "Placed" with items as follows:
	| Product ID | Quantity |
	| BOOK-1     |      150 |
	| BOOK-2     |      120 |
	And product "BOOK-1" available quantity becomes 200
	And product "BOOK-2" available quantity becomes 20

Scenario: Add quantity of existing product in Order
	Given All flows are configured
	And product with SKU "BOOK-1" and available quantity 350 is present in inventory
	And having placed order for "ABC ltd", date "11/12/2025", ID "O2025/1324" and products as follows:
	| Product ID | Quantity |
	| BOOK-1     |      150 |
	When updating order "O2025/1324" product changes as follows:
	| Product ID | Quantity |
	| BOOK-1     |     +120 |
	Then order "O2025/1324" has status "Placed" with items as follows:
	| Product ID | Quantity |
	| BOOK-1     |      270 |
	And product "BOOK-1" available quantity becomes 80

Scenario: Reduce quantity of existing product in Order
	Given All flows are configured
	And product with SKU "BOOK-1" and available quantity 350 is present in inventory
	And having placed order for "ABC ltd", date "11/12/2025", ID "O2025/1324" and products as follows:
	| Product ID | Quantity |
	| BOOK-1     |      150 |
	When updating order "O2025/1324" product changes as follows:
	| Product ID | Quantity |
	| BOOK-1     |     -120 |
	Then order "O2025/1324" has status "Placed" with items as follows:
	| Product ID | Quantity |
	| BOOK-1     |       30 |
	And product "BOOK-1" available quantity becomes 320
