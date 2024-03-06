import { movieDetailsPage as Page } from '@/integration/page-objects';

describe('Movie Details Buttons', () => {
	it('Movie Requested by Admin should be auto approved', () => {
		cy.login();

		Page.visit('587807');
		Page.requestButton.click();
		Page.adminOptionsDialog.isOpen();

		Page.adminOptionsDialog.requestButton.click();

		cy.verifyNotification('Request for Tom & Jerry has been added successfully');

		Page.requestedButton.should('be.visible');
	});

	it('Movie Requested by Regular user should be pending', () => {
		cy.generateUniqueId().then((id) => {
			cy.login();
			const roles = [];
			roles.push({ value: 'RequestMovie', enabled: true });
			cy.createUser(id, 'a', roles).then(() => {
				cy.loginWithCreds(id, 'a');

				Page.visit('651571');

				Page.requestButton.click();
				cy.verifyNotification('Request for Breach has been added successfully');

				Page.requestedButton.should('be.visible');
			});
		});
	});

	it('Movie Requested by Regular with no movie permission', () => {
		cy.generateUniqueId().then((id) => {
			cy.login();
			const roles = [];
			roles.push({ value: 'RequestTv', enabled: true });
			cy.createUser(id, 'a', roles).then(() => {
				cy.loginWithCreds(id, 'a');

				Page.visit('791373');

				Page.requestButton.click();
				cy.verifyNotification('You do not have permissions to Request a Movie');

				Page.requestedButton.should('not.exist');
			});
		});
	});

	it('Movie Requested by Regular can be approved by admin', () => {
		cy.generateUniqueId().then((id) => {
			cy.login();
			const roles = [];
			roles.push({ value: 'RequestMovie', enabled: true });
			cy.createUser(id, 'a', roles).then(() => {
				cy.loginWithCreds(id, 'a');

				Page.visit('793723');

				Page.requestButton.click();
				cy.verifyNotification('Request for Sentinelle has been added successfully');

				Page.requestedButton.should('be.visible');

				// Login as admin now
				cy.removeLogin();
				cy.login();
				cy.reload();

				cy.intercept('GET', '**/Request/movie/info/**').as('requestCall');

				Page.visit('793723');

				cy.wait('@requestCall').then((__) => {
					Page.approveButton.should('exist');
					Page.approveButton.click();

					cy.verifyNotification('Successfully Approved');
				});
			});
		});
	});

	it('Movie Requested, mark as available', () => {
		cy.login();

		Page.visit('12444');

		Page.requestButton.click();
		Page.adminOptionsDialog.isOpen();
		Page.adminOptionsDialog.requestButton.click();
		cy.verifyNotification('Request for Harry Potter and the Deathly Hallows: Part 1 has been added successfully');

		cy.intercept('GET', '**/Images/banner/movie/**').as('bannerLoad');
		cy.reload();

		cy.wait('@bannerLoad').then((__) => {
			Page.markAvailableButton.should('exist');
			Page.markAvailableButton.click();

			cy.verifyNotification('Request is now available');
			Page.availableButton.should('exist');
		});
	});

	it.skip('Movie Requested, Deny Movie', () => {
		cy.login();

		Page.visit('671');

		Page.requestButton.click();
		Page.adminOptionsDialog.isOpen();
		Page.adminOptionsDialog.requestButton.click();
		cy.verifyNotification("Request for Harry Potter and the Philosopher's Stone has been added successfully");

		cy.reload();

		Page.denyButton.should('exist');
		Page.denyButton.click();

		Page.denyModal.denyReason.type('Automation Tests');
		cy.wait(500);
		Page.denyModal.denyButton.click();

		Page.deniedButton.should('exist');

		cy.verifyNotification('Denied Request');

		cy.wait(1000);
		Page.informationPanel.denyReason.should('have.text', 'Automation Tests');
	});

	it('Movie View Collection should be available', () => {
		cy.login();

		Page.visit('671');

		Page.viewCollectionButton.should('be.visible');
	});

	it('Non requested movie valid buttons', () => {
		cy.login();

		Page.visit('590706');

		Page.viewCollectionButton.should('not.exist');
		Page.approveButton.should('not.exist');
		Page.denyButton.should('not.exist');
		Page.deniedButton.should('not.exist');
		Page.markAvailableButton.should('not.exist');
		Page.viewOnEmbyButton.should('not.exist');
		Page.viewOnJellyfinButton.should('not.exist');
		Page.viewOnPlexButton.should('not.exist');
		Page.requestedButton.should('not.exist');
		Page.reportIssueButton.should('not.exist'); // Issuess not enabled
		Page.requestButton.should('exist');
	});
});
