import { movieDetailsPage as Page } from "@/integration/page-objects";

describe("TV Search V1 API tests", () => {
    beforeEach(() => {
        cy.login();
    });

    it("Get Extra TV Info", () => {
        cy.api({url: '/api/v1/search/tv/info/121361', headers: { 'Authorization': 'Bearer ' + window.localStorage.getItem('id_token')} })
            .then((res) => {
                expect(res.status).equal(200);
                cy.fixture('api/v1/tv-search-extra-info').then(x => {
                    expect(res.body).deep.equal(x);
                })
            });
      });

      it("Popular TV", () => {
        cy.api({url: '/api/v1/search/tv/popular', headers: { 'Authorization': 'Bearer ' + window.localStorage.getItem('id_token')} })
            .then((res) => {
                expect(res.status).equal(200);
                const body = res.body;
                expect(body.length).is.greaterThan(0);
                expect(body[0].title).is.not.null;
                expect(body[0].id).is.not.null;
                expect(body[0].id).to.be.an('number');
            });
      });

 
});
