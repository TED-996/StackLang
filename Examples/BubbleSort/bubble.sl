: n = m10
: m0--m5 = indices
: m6 = done?
: m20+ = v

<< m10 = :read m
0 m0 =
m0 m10 < r = \\ k+4 . k r if ; :read values
	<< m0 20 + m =
	m0 1 + m0 =
p p k-3
1 m1 =
m1 m10 < m6 ! && :loop n - 1 times or until m6 = done is true
r = \\ k+13 . k r if ;
	0 m0 =
	1 m6 =
	m0 m10 m1 - < r = \\ k+8 . k r if ; :loop n - i times
		m0 20 + m m0 21 + m > r = \\ k+5 . k r if ; :if v[i] > v[i + 1]
			m0 20 + m r = : swap values
			m0 21 + m m0 20 + m =
			r m0 21 + m =
			0 m6 = ; :set done to false
		m0 1 + m0 =
	p p k-7
	m1 1 + m1 =
p p k-13
:print v
0 m0 =
m0 m10 < r = \\ k+4 . k r if ;
	m0 20 + m >>
	m0 1 + m0 =
p p k-3

	
