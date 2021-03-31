import { userPreferencesPage as Page } from "@/integration/page-objects";

describe("User Preferences Security Tests", () => {
  beforeEach(() => {
    cy.login();
    Page.visit();
    Page.securityTab.click();
  });


  it(`Change Email Address Requires Current Password`, () => {
    Page.security.email.clear();
    Page.security.email.type('test@test.com');
    Page.security.submitButton.click();

    Page.security.currentPassword.should('have.class', 'ng-invalid');
  });

  it(`Change Password Requires Current Password`, () => {
    Page.security.newPassword.type('test@test.com');
    Page.security.confirmPassword.type('test@test.com');
    Page.security.submitButton.click();

    Page.security.currentPassword.should('have.class', 'ng-invalid');
  });

  it(`Change Email incorrect password`, () => {
    Page.security.currentPassword.type('incorrect');
    Page.security.email.clear();
    Page.security.email.type('test@test.com');
    Page.security.submitButton.click();
    cy.verifyNotification('password is incorrect');
  });

  it(`Change password, existing password incorrect`, () => {
    Page.security.currentPassword.type('incorrect');
    Page.security.newPassword.type('test@test.com');
    Page.security.confirmPassword.type('test@test.com');
    Page.security.submitButton.click();
    cy.verifyNotification('password is incorrect');
  });

  it("Change password of user", () => {
    cy.generateUniqueId().then((id) => {
      const roles = [];
      roles.push({ value: "RequestMovie", enabled: true });
      cy.createUser(id, "a", roles).then(() => {
        cy.removeLogin();
        cy.loginWithCreds(id, "a");

        Page.visit();
        Page.securityTab.click();

        Page.security.currentPassword.type('a');
        Page.security.email.clear();
        Page.security.email.type('test@test.com');
        Page.security.newPassword.type('b');
        Page.security.confirmPassword.type('b');
        Page.security.submitButton.click();

        cy.verifyNotification('Updated your information');

        Page.email.should('have.text','(test@test.com)')
      });
    });
  });
});


