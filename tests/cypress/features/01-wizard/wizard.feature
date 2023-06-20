
Feature: Wizard Setup
  Scenario: When visiting Ombi for the first time we should be on the Wizard page
    When I visit Ombi
    Then I should be on the "Wizard"

  Scenario: When navigating through the Wizard feature we are required to create a local user
    When I visit Ombi
    And I click through all of the pages
    And I finish the Wizard
    Then I should get a notification "Username '' is invalid, can only contain letters or digits."
    And I should be on the User tab

  Scenario: Completing the Wizard
    When I visit Ombi
    And I click through to the user page
    And I enter a username
    And I enter a password
    And I go to the finished tab
    And I finish the Wizard
    Then I should be on the "login"