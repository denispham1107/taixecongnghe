"""21.91: shared authored-metre details; merged into existing meshes and atlas.
Loaded into the street generator namespace (Geo/Vector/CI available).
No scene, material, export or random-state side effects.
"""
def fixture_case(g, c, s, bevel, col, tag='Fixture'):
    x,y,z=c; w,h,t=s; r=min(bevel,w/4,h/4)
    p=[(-w/2+r,-h/2),(w/2-r,-h/2),(w/2,-h/2+r),(w/2,h/2-r),
       (w/2-r,h/2),(-w/2+r,h/2),(-w/2,h/2-r),(-w/2,-h/2+r)]
    vs=[(x+a,y+b,z+side*t/2) for side in (-1,1) for a,b in p]
    fs=[tuple(range(7,-1,-1)),tuple(range(8,16))]+[(i,(i+1)%8,(i+1)%8+8,i+8) for i in range(8)]
    g.add(vs,fs,col,('c',c),tag)

def fixture_ring(g,x,y,z,outer,inner,col,n,tag='Fixture'):
    vs=[(x+r*cos(i*2*pi/n),y+r*sin(i*2*pi/n),z) for r in (outer,inner) for i in range(n)]
    g.add(vs,[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)],col,('d',(0,0,-1)),tag)

def condenser(g,x,y,z=-4.12,f=-1,scale=1):
    """Soft grey housing, recessed impeller, real guard, louvres and L brackets."""
    d=g.d; local=Geo(d)
    fixture_case(local,(0,0,0),(.75,.55,.42),.035,'chrome','AC')
    fixture_case(local,(0,0,-.219),(.70,.50,.022),.025,'steel','AC')
    local.disc((-.105,0,-.234),.191,'darkFrame',n=(24,16,10)[d],tag='AC')
    if d<2:
        # Broad swept fan blades behind the grill, with no animated shimmer.
        for i in range(5):
            a=2*pi*i/5
            pts=[(-.105+r*cos(a+b),r*sin(a+b),-.239) for r,b in ((.025,0),(.14,.12),(.166,.55),(.11,.78),(.035,.55))]
            local.add(pts,[(0,1,2,3,4)],'ironGrey',('d',(0,0,-1)),'AC')
        fixture_ring(local,-.105,0,-.248,.192,.178,'chrome',(24,16)[d],'AC')
        for r in ((.06,.10,.14,.175) if d==0 else (.10,.17)):
            fixture_ring(local,-.105,0,-.253,r+.003,r-.003,'steel',(24,16)[d],'AC')
        for i in range(8 if d==0 else 4):
            a=2*pi*i/(8 if d==0 else 4)
            local.cyl((-.105+.027*cos(a),.027*sin(a),-.255),(-.105+.174*cos(a),.174*sin(a),-.255),.0035,'steel',n=3,tag='AC')
        local.disc((-.105,0,-.258),.025,'steel',n=10,tag='AC')
        local.cube((.239,0,-.237),(.137,.35,.012),'ironGrey',tag='AC')
        for i in range(7 if d==0 else 4):
            local.cube((.239,-.14+i*.28/(6 if d==0 else 3),-.252),(.125,.012,.022),'chrome',tag='AC')
        local.cube((.24,.208,-.241),(.12,.023,.008),'navy',tag='AC')
        for sx in (-.245,.245):
            local.cube((sx,-.301,.065),(.037,.035,.52),'ironGrey',tag='AC')
            local.cube((sx,-.20,.303),(.037,.23,.035),'ironGrey',tag='AC')
            local.cube((sx,-.278,.015),(.09,.012,.16),'rubber',tag='AC')
        if d==0:
            for sx in (-.315,.315):
                for sy in (-.205,.205):local.disc((sx,sy,-.238),.008,'ironGrey',n=6,tag='AC')
            local.cyl((.355,-.10,.15),(.41,-.1,.23),.012,'chrome',n=6,tag='AC')
            local.cyl((.41,-.1,.23),(.41,-.38,.23),.015,'rubber',n=6,tag='AC')
    else:
        fixture_ring(local,-.105,0,-.24,.19,.171,'steel',10,'AC')
        local.cube((.24,0,-.236),(.12,.34,.012),'ironGrey',tag='AC')
        local.cube((0,-.30,.02),(.60,.04,.50),'ironGrey',tag='AC')
    # Mirror only depth for a facade facing +z; hints are reflected with it.
    sign=-f
    pts=[(x+a*scale,y+b*scale,z+c*scale*sign) for a,b,c in local.v]
    hints=[]
    for typ,p in local.hint:
        hints.append((typ,(x+p[0]*scale,y+p[1]*scale,z+p[2]*scale*sign) if typ=='c' else (p[0],p[1],p[2]*sign)))
    # Faces already reference local vertex indices, so add the whole batch once.
    start=len(g.v);g.v.extend(pts)
    g.f.extend(tuple(start+i for i in face) for face in local.f)
    g.cols.extend(local.cols);g.tags.extend(local.tags);g.hint.extend(hints)

def detailed_traffic_signal(detail):
    g=Geo(detail);n=(16,12,8)[detail];hx,hy=SIGNAL_REACH,SIGNAL_HEAD_Y
    fixture_case(g,(0,.15,0),(.46,.30,.46),.05,'ironGrey')
    g.cyl((0,.3,0),(0,7.65,0),.105,'steel',n=n,r2=.085)
    for y in (.35,1.5,6.85,7.58):g.cyl((0,y,0),(0,y+.055,0),.12,'chrome',n=n)
    # Swept elbow into a tapered horizontal mast, not a bare right-angle joint.
    p=[(0,7.55,0)]+[(.30-.30*cos(pi*i/12),7.55+.30*sin(pi*i/12),0) for i in range(1,7)]
    p+=[(hx+.32,7.85,0)]
    for a,b in zip(p,p[1:]):g.cyl(a,b,.075,'steel',n=n,r2=.069)
    g.cyl((0,6.85,0),(1.48,7.85,0),.027,'ironGrey',n=6)
    for yy in (hy+.6,hy-.6):
        g.cube((hx,yy,.23),(.29,.06,.08),'steel')
    g.cyl((hx,7.84,.19),(hx,hy-.62,.19),.028,'steel',n=6)
    fixture_case(g,(hx,hy,.07),(.80,1.78,.055),.095,'steel')
    fixture_case(g,(hx,hy,.026),(.75,1.72,.025),.075,'iron')
    fixture_case(g,(hx,hy,-.022),(.56,1.48,.37),.065,'darkFrame')
    # Keep all runtime red/amber/green lens centres and radii unchanged.
    for k in range(3):
        yy=hy+.43-k*.43
        fixture_ring(g,hx,yy,-.213,.187,.154,'ironGrey',(24,16,10)[detail])
        if detail<2:
            # Curved open-front sun visor; no geometry covers the active lens.
            seg=(12,8)[detail];r=.197
            vs=[(hx+r*cos(a),yy+r*sin(a),zz) for zz in (-.209,-.48) for a in [-.18+(pi+.36)*i/seg for i in range(seg+1)]]
            g.add(vs,[(i,i+1,i+seg+2,i+seg+1) for i in range(seg)],'iron',('c',(hx,yy,-.33)))
            fixture_case(g,(hx+.19,yy,.178),(.042,.12,.018),.009,'steel')
        g.disc((hx,yy,.169),.145,'ironGrey',facing=(0,0,1),n=(16,12,8)[detail])
    fixture_case(g,(0,3.55,-.16),(.34,.94,.24),.035,'darkFrame')
    for k in range(3):
        yy=3.85-k*.3
        g.disc((0,yy,-.286),.102,'ironGrey',n=(16,12,8)[detail])
        if detail==0:fixture_ring(g,0,yy,-.29,.115,.10,'steel',16)
    fixture_case(g,(-.14,1.28,.03),(.22,.55,.32),.025,'steel')
    if detail==0:
        for sx in (-.15,.15):
            for sz in (-.15,.15):g.cyl((sx,.30,sz),(sx,.34,sz),.021,'chrome',n=6)
        for yy in (hy-.56,hy+.56):g.disc((hx+.23,yy,-.212),.012,'steel',n=8)
    g.meta=dict(obstacles=[dict(kind='Cột đèn tín hiệu',center=[0,3.9,0],size=[.3,7.8,.3])],head_center=[SIGNAL_REACH,SIGNAL_HEAD_Y,0])
    return g

def rental_sign(g,x0,x1,zf,f,floors):
    """Exactly one plaque per tower, chosen without consuming its layout RNG."""
    cx=(x0+x1)/2;seed=int(abs(cx)*131+abs(zf)*997)
    floor=1+seed%max(1,floors-1);y0=.2+3.6+(floor-1)*3
    w=min((x1-x0)*.68,9.0);h=.88;z=zf-f*HOUSE_DEPTH-f*.07
    g.cube((cx,y0+1.85,z),(w,h,.045),'navy',tag='RentalSign')
    g.panel((cx,y0+1.85,z-f*.025),w-.10,h-.10,'navy',z_facing=-f,tag='RentalSign')
    g.cube((cx,y0+1.85+h/2-.035,z-f*.029),(w,.055,.012),'chrome',tag='RentalSign')
    g.cube((cx,y0+1.85-h/2+.035,z-f*.029),(w,.055,.012),'chrome',tag='RentalSign')
    g.text('CHO THUÊ NHÀ',(cx,y0+1.85,z-f*.04),.58,'white',maxwidth=w-.5,facing=-f,tag='RentalSign')
    for xx in (cx-w/2+.11,cx+w/2-.11):
        g.disc((xx,y0+1.85,z-f*.034),.025,'chrome',facing=(0,0,-f),n=6,tag='RentalSign')
    info=dict(label='CHO THUÊ NHÀ',floor=floor,center=[cx,y0+1.85,z],width=w,height=h,facing=-f)
    g.rental_signs=getattr(g,'rental_signs',[])+[info]
