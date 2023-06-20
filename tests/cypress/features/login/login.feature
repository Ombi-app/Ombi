
Feature: Login Page
  Scenario: When visiting Ombi and the Landing Page is enabled, we should end up on the landing page
    Given I set the Landing Page to "true"
    When I visit Ombi
    Then I should be on the "landingpage"
    Then I click continue
    Then I should be on the "login/true"

  Scenario: When visiting Ombi and the Landing Page is disabled, we should end up on the login page
    Given I set the Landing Page to "false"
    When I visit Ombi
    Then I should be on the "login"