import { movieDetailsPage as Page } from "@/integration/page-objects";

describe("TV Search V1 API tests", () => {
    beforeEach(() => {
        cy.login();
    });

    it("Get Extra TV Info", () => {
        cy.api({url: '/api/v1/search/tv/info/287247', headers: { 'Authorization': 'Bearer ' + window.localStorage.getItem('id_token')} })
            .then((res) => {
                expect(res.status).equal(200);
                cy.fixture('api/v1/tv-search-extra-info').then(x => {
                    expect(res.body).deep.equal(x);
                })
            });
      });

    it("TV Basic Search", () => {
        cy.api({url: '/api/v1/search/tv/Shitts Creek', headers: { 'Authorization': 'Bearer ' + window.localStorage.getItem('id_token')} })
            .then((res) => {
                expect(res.status).equal(200); 
                const body = res.body;
                expect(body[0].title).is.equal("Schitt's Creek")
                expect(body[0].status).is.equal("Ended");
                expect(body[0].id).is.not.null;
                expect(body[0].id).to.be.an('number');
            });
      });

const types = [
    'popular',
    'trending',
    'anticipated',
    'mostwatched'
  ];

  types.forEach((type) => {
      // derive test name from data
      it(`${type} TV List`, () => {
        cy.api({url: '/api/v1/search/tv/'+type, headers: { 'Authorization': 'Bearer ' + window.localStorage.getItem('id_token')} })
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
});
