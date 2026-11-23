/*
 * This file is part of veraPDF Parser, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Parser is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Parser as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Parser as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.cos.xref;

import org.verapdf.cos.COSKey;

import java.util.*;

/**
 * Holds cross-reference entries ordered by object number and provides helpers
 * to merge active/free entries and group consecutive numbers into ranges.
 *
 * <h2>.NET porting notes</h2>
 * In C#, represent {@code COSXRefSection} with immutable records containing numeric
 * object/generation identifiers and source offsets. Keep the xref table sparse (for example,
 * {@code IReadOnlyDictionary<COSKey, COSXRefEntry>}) and provide deterministic range/section
 * validation before resolving objects.
 */
public class COSXRefSection {

	private final Map<Integer, COSXRefEntry> entries;

	public COSXRefSection() {
		this.entries = new TreeMap<>();
		this.entries.put(0, COSXRefEntry.FIRST_XREF_ENTRY);
	}

	public void add(final COSKey key, final long offset) {
		this.add(key, offset, 'n');
	}

	/**
	 * Adds or replaces the entry for an object number.
	 *
	 * @param key object number and generation
	 * @param offset object offset, or next-free reference for a free entry
	 * @param free {@code 'n'} for in-use or {@code 'f'} for free
	 */
	public void add(final COSKey key, final long offset, final char free) {
		this.entries.put(key.getNumber(), new COSXRefEntry(offset, key.getGeneration(), free));
	}

	public void add(final Map<COSKey, Long> offsets) {
		this.add(offsets, 'n');
	}

	public void add(final Map<COSKey, Long> offsets, final char free) {
		for (Map.Entry<COSKey, Long> entry : offsets.entrySet()) {
			this.add(entry.getKey(), entry.getValue(), free);
		}
	}

	/**
	 * Adds this section's in-use entries to {@code keys} and applies its
	 * cross-reference free-entry handling.
	 *
	 * @param keys keys accumulated from another cross-reference revision
	 */
	public void addTo(final List<COSKey> keys) {
		for (Map.Entry<Integer, COSXRefEntry> entry : this.entries.entrySet()) {
			final COSKey key = new COSKey(entry.getKey(), entry.getValue().generation);
			if (entry.getValue().free == 'n') {
				keys.add(key);
			} else {
				removeIfNumberEqual(keys, key.getNumber());
			}
		}
	}

	/**
	 * Merges this section into accumulated object offsets. In-use entries are
	 * added, while free entries remove the preceding generation of that object
	 * number.
	 *
	 * @param offsets offsets accumulated from another cross-reference revision
	 */
	public void addTo(final Map<COSKey, Long> offsets) {
		for (Map.Entry<Integer, COSXRefEntry> entry : this.entries.entrySet()) {
			final COSKey key = new COSKey(entry.getKey(), entry.getValue().generation);
			if (entry.getValue().free == 'n') {
				offsets.put(key, entry.getValue().offset);
			} else {
				offsets.remove(new COSKey(key.getNumber(), key.getGeneration() - 1));
			}
		}
	}

	/**
	 * Groups the stored entry numbers into sorted, contiguous ranges.
	 *
	 * @return ranges suitable for a cross-reference stream's {@code /Index}
	 */
	public List<COSXRefRange> getRange() {
		List<COSXRefRange> result = new ArrayList<>();

		if (this.entries.isEmpty()) {
			return result;
		}

		Iterator<Map.Entry<Integer, COSXRefEntry>> iterator = this.entries.entrySet().iterator();
		COSXRefRange segment = new COSXRefRange(iterator.next().getKey());
		while(iterator.hasNext()) {
			int nextSegment = iterator.next().getKey();
			if (nextSegment == segment.next()) {
				segment.count++;
			} else {
				result.add(segment);
				segment = new COSXRefRange(nextSegment);
			}
		}
		result.add(segment);

		return result;
	}

	/**
	 * Looks up the cross-reference entry for an object number.
	 *
	 * @param number object number
	 * @return its entry, or {@code null} if this section has no such entry
	 */
	public COSXRefEntry getEntry(final int number) {
		return this.entries.get(number);
	}

	/**
	 * Adds or replaces a cross-reference entry by object number.
	 *
	 * @param number object number
	 * @param entry cross-reference entry to store
	 */
	public void addEntry(final int number, final COSXRefEntry entry) {
		this.entries.put(number, entry);
	}

	private static void removeIfNumberEqual(final List<COSKey> keys, final int number) {
		for (COSKey key : keys) {
			if (key.getNumber() == number) {
				keys.remove(number);
			}
		}
	}

	/**
	 * Returns the first object number after the greatest entry in this section,
	 * or 1 when the section is empty.
	 *
	 * @return next object number
	 */
	public long next() {
		if (entries.isEmpty()) {
			return 1;
		} else {
			// TODO : no nice way to get last element of linked hash map
			Iterator<Map.Entry<Integer, COSXRefEntry>> iterator = this.entries.entrySet().iterator();
			Map.Entry<Integer, COSXRefEntry> lastElement = iterator.next();
			while (iterator.hasNext()) {
				lastElement = iterator.next();
			}
			return lastElement.getKey() + 1;
		}
	}

}
