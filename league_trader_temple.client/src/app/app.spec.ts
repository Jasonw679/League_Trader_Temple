import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { CommonModule } from '@angular/common';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { App } from './app';
import { Header } from './header/header';

describe('App', () => {
  let component: App;
  let fixture: ComponentFixture<App>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [App, Header],
      imports: [CommonModule, HttpClientTestingModule]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(App);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create the app', () => {
    expect(component).toBeTruthy();
  });

  it('should retrieve Riftbound cards from the server', () => {
    const mockCardPage = {
      items: [
        createCard('Jinx Rebel', 'OGN-001/298'),
        createCard('Vi Destructive', 'OGN-002/298')
      ],
      total: 2,
      page: 1,
      size: 24,
      pages: 1
    };

    component.ngOnInit();

    const req = httpMock.expectOne((request) => request.url === '/riftboundcards');
    expect(req.request.method).toEqual('GET');
    expect(req.request.params.get('setId')).toBe('ogn');
    req.flush(mockCardPage);

    expect(component.cards()).toEqual(mockCardPage.items);
    expect(component.totalCards()).toBe(2);
  });

  it('should render Riftbound cards after the server responds', () => {
    const mockCardPage = {
      items: [
        createCard('Jinx Rebel', 'OGN-001/298'),
        createCard('Vi Destructive', 'OGN-002/298')
      ],
      total: 2,
      page: 1,
      size: 24,
      pages: 1
    };

    fixture.detectChanges();

    const req = httpMock.expectOne((request) => request.url === '/riftboundcards');
    req.flush(mockCardPage);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelectorAll('.riftbound-card').length).toBe(2);
    expect(compiled.textContent).toContain('Jinx Rebel');
  });

  it('should expose an error when Riftbound cards fail to load', () => {
    component.ngOnInit();

    const req = httpMock.expectOne((request) => request.url === '/riftboundcards');
    req.flush('Server unavailable', { status: 500, statusText: 'Server Error' });

    expect(component.cards()).toEqual([]);
    expect(component.cardLoadError()).toContain('Unable to load Riftbound cards');
  });
});

function createCard(name: string, publicCode: string) {
  return {
    id: name.toLowerCase().replaceAll(' ', '-'),
    name,
    riftboundId: publicCode.toLowerCase(),
    publicCode,
    collectorNumber: 1,
    attributes: {},
    classification: {
      type: 'Unit',
      rarity: 'Rare',
      domain: ['Chaos']
    },
    set: {
      setId: 'OGN',
      label: 'Origins'
    },
    media: {
      imageUrl: 'https://example.com/card.png',
      artist: 'Riot Games',
      accessibilityText: name
    }
  };
}
