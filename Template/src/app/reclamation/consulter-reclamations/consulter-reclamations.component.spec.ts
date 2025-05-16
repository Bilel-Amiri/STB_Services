import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConsulterReclamationsComponent } from './consulter-reclamations.component';

describe('ConsulterReclamationsComponent', () => {
  let component: ConsulterReclamationsComponent;
  let fixture: ComponentFixture<ConsulterReclamationsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ConsulterReclamationsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ConsulterReclamationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
