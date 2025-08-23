import { BasePage } from "../base.page";

export class LoginPage extends BasePage {
  // Page selectors
  private readonly selectors = {
    username: '#username-field',
    password: '#password-field',
    ombiSignInButton: '[data-cy=OmbiButton]',
    plexSignInButton: '[data-cy=oAuthPlexButton]',
    loginForm: '[data-test="login-form"]',
    errorMessage: '[data-test="error-message"]',
    loadingSpinner: '[data-test="loading-spinner"]'
  } as const;

  // Getters for page elements
  get username() {
    return cy.get(this.selectors.username);
  }

  get password() {
    return cy.get(this.selectors.password);
  }

  get ombiSignInButton() {
    return cy.get(this.selectors.ombiSignInButton);
  }

  get plexSignInButton() {
    return cy.get(this.selectors.plexSignInButton);
  }

  get loginForm() {
    return cy.get(this.selectors.loginForm);
  }

  get errorMessage() {
    return cy.get(this.selectors.errorMessage);
  }

  get loadingSpinner() {
    return cy.get(this.selectors.loadingSpinner);
  }

  // Page visit method
  visit(options?: any): Cypress.Chainable<any> {
    this.logAction('Visiting login page');
    return cy.visit('/login', options);
  }

  // Page-specific methods
  /**
   * Fill login form with credentials
   */
  fillLoginForm(username: string, password: string): Cypress.Chainable<any> {
    this.logAction('Filling login form', { username });
    
    return cy.wrap(null).then(() => {
      this.username.clear().type(username);
      this.password.clear().type(password);
    });
  }

  /**
   * Submit login form
   */
  submitLoginForm(): Cypress.Chainable<any> {
    this.logAction('Submitting login form');
    return this.ombiSignInButton.click();
  }

  /**
   * Complete login flow
   */
  login(username: string, password: string): Cypress.Chainable<any> {
    this.logAction('Performing login', { username });
    
    return this.fillLoginForm(username, password)
      .then(() => this.submitLoginForm());
  }

  /**
   * Wait for login to complete
   */
  waitForLoginComplete(): Cypress.Chainable<any> {
    this.logAction('Waiting for login to complete');
    
    return cy.url().should('not.include', '/login')
      .then(() => {
        this.logAction('Login completed successfully');
      });
  }

  /**
   * Check if login form is visible
   */
  isLoginFormVisible(): Cypress.Chainable<any> {
    return this.loginForm.should('be.visible').then(() => true);
  }

  /**
   * Check if Plex OAuth button is visible
   */
  isPlexOAuthVisible(): Cypress.Chainable<any> {
    return this.plexSignInButton.should('be.visible').then(() => true);
  }

  /**
   * Check if error message is displayed
   */
  isErrorMessageVisible(): Cypress.Chainable<any> {
    return this.errorMessage.should('be.visible').then(() => true);
  }

  /**
   * Get error message text
   */
  getErrorMessageText(): Cypress.Chainable<any> {
    return this.errorMessage.invoke('text');
  }

  /**
   * Clear login form
   */
  clearLoginForm(): Cypress.Chainable<any> {
    this.logAction('Clearing login form');
    
    return cy.wrap(null).then(() => {
      this.username.clear();
      this.password.clear();
    });
  }

  /**
   * Verify page is loaded
   */
  verifyPageLoaded(): Cypress.Chainable<any> {
    this.logAction('Verifying login page is loaded');
    
    return this.waitForPageLoad()
      .then(() => this.isLoginFormVisible())
      .then(() => {
        this.logAction('Login page loaded successfully');
      });
  }
}

export const loginPage = new LoginPage();
