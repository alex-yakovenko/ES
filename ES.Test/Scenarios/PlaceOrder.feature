Feature: PlaceOrder
New Order Placing with reservation of products in the inventory

  @tag1
  Scenario: Successful order placing
    Given All flows are configured
    And product with SKU "BOOK-1" and available quantity 350 is present in inventory
    And product with SKU "BOOK-2" and available quantity 140 is present in inventory
    When placing order for "ABC ltd", date "11/12/2025", ID "O2025/1324" and products as follows:
      | Product ID | Quantity |
      | BOOK-1     |      150 |
      | BOOK-2     |      120 |
    Then order "O2025/1324" has status "Placed" with items as follows:
      | Product ID | Quantity |
      | BOOK-1     |      150 |
      | BOOK-2     |      120 |
    And product "BOOK-1" available quantity becomes 200
    And product "BOOK-2" available quantity becomes 20

  Scenario: Not enough product order placing
    Given All flows are configured
    And product with SKU "BOOK-1" and available quantity 350 is present in inventory
    And product with SKU "BOOK-2" and available quantity 100 is present in inventory
    When placing order for "ABC ltd", date "11/12/2025", ID "O2025/1324" and products as follows:
      | Product ID | Quantity |
      | BOOK-1     |      150 |
      | BOOK-2     |      120 |
    Then order "O2025/1324" has status "Cancelled" with items as follows:
      | Product ID | Quantity |
      | BOOK-1     |      150 |
      | BOOK-2     |      120 |
    And product "BOOK-1" available quantity becomes 350
    And product "BOOK-2" available quantity becomes 100
